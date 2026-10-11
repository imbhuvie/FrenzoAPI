using FrenZoAPI.Common.Responses;
using FrenZoAPI.DataContext;
using FrenZoAPI.DTOs.Chat;
using FrenZoAPI.Models;
using FrenZoAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrenZoAPI.Services;

// Builds the chatlist UI rows in a few batched reads (no N+1, no tracking).
public class ConversationService : IConversationService
{
    // Page-size bounds for the chatlist (R9: named, one place to tune).
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 50;

    private readonly AppDbContext _db;

    public ConversationService(AppDbContext db)
    {
        _db = db;
    }

    // Returns the caller's conversations newest-first with last-message previews.
    // No try-catch here: read-only path, unexpected failures go to the middleware (R11).
    public async Task<ApiResponse<List<ChatlistItemResponse>>> GetChatlistAsync(Guid callerId, ChatlistQuery query)
    {
        int pageSize = ClampPageSize(query.PageSize);
        DateTime? cursorWall = NormalizeCursor(query.Cursor);
        DateTime nowWall = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        // Caller memberships only; left or soft-deleted rows never list (R6: calls stay out of conditions).
        IQueryable<TblConversationParticipant> memberships = _db.TblConversationParticipants
            .AsNoTracking()
            .Where(p => p.UserId == callerId && !p.IsDelete && p.LeftAt == null);

        // One projected page keyed by scalars only: entities in a projection ctor break
        // Npgsql translation, so the row carries fields—not tracked entities (see log 2026-10-11).
        IQueryable<PageRow> projected = from member in memberships
                        join conversation in _db.TblConversations.AsNoTracking().Where(c => !c.IsDelete)
                            on member.ConversationId equals conversation.Id
                        let lastMessage = _db.TblMessages.AsNoTracking()
                            .Where(m => m.ConversationId == conversation.Id && !m.IsDelete)
                            .OrderByDescending(m => m.CreatedAt)
                            .FirstOrDefault()
                        let groupProfile = _db.TblGroupProfiles.AsNoTracking()
                            .Where(g => g.ConversationId == conversation.Id && !g.IsDelete)
                            .FirstOrDefault()
                        let activityAt = lastMessage != null ? lastMessage.CreatedAt : conversation.CreatedAt
                        orderby activityAt descending
                        select new PageRow(
                            member.ConversationId,
                            member.IsAdmin,
                            member.MutedUntil,
                            member.JoinedAt,
                            member.LastReadMessageId,
                            conversation.Type,
                            conversation.CreatedAt,
                            groupProfile != null ? groupProfile.GroupName : null,
                            groupProfile != null ? groupProfile.GroupPhotoUrl : null,
                            lastMessage != null ? (Guid?)lastMessage.Id : null,
                            lastMessage != null ? lastMessage.Content : null,
                            lastMessage != null ? (Guid?)lastMessage.SenderId : null,
                            lastMessage != null ? (short?)lastMessage.MessageType : null,
                            lastMessage != null ? (DateTime?)lastMessage.CreatedAt : null,
                            activityAt);

        bool hasCursor = cursorWall.HasValue;
        IQueryable<PageRow> filtered = projected;
        if (hasCursor)
            filtered = projected.Where(r => r.ActivityAt < cursorWall!.Value);

        List<PageRow> rows = await filtered
            .Take(pageSize)
            .ToListAsync();

        bool isEmpty = rows.Count == 0;
        if (isEmpty)
            return ApiResponse<List<ChatlistItemResponse>>.Ok(new List<ChatlistItemResponse>(), "Chatlist fetched.");

        Guid[] conversationIds = rows.Select(r => r.ConversationId).ToArray();
        Dictionary<Guid, PeerInfo> peersByConversation = await LoadPeersAsync(conversationIds, callerId);
        Dictionary<Guid, int> unreadByConversation = await LoadUnreadAsync(conversationIds, callerId);

        List<ChatlistItemResponse> items = new List<ChatlistItemResponse>(rows.Count);
        foreach (PageRow row in rows)
        {
            ChatlistItemResponse item = MapRow(row, peersByConversation, unreadByConversation, nowWall);
            items.Add(item);
        }

        return ApiResponse<List<ChatlistItemResponse>>.Ok(items, "Chatlist fetched.");
    }

    // First active peer per conversation with display name and photo (one batched LEFT JOIN).
    private async Task<Dictionary<Guid, PeerInfo>> LoadPeersAsync(Guid[] conversationIds, Guid callerId)
    {
        List<PeerRow> peerRows = await (
            from participant in _db.TblConversationParticipants.AsNoTracking()
            where conversationIds.Contains(participant.ConversationId) && participant.UserId != callerId && !participant.IsDelete && participant.LeftAt == null
            join user in _db.TblUsers.AsNoTracking().Where(u => !u.IsDelete)
                on participant.UserId equals user.Id
            join profile in _db.UserProfiles.AsNoTracking().Where(p => !p.IsDelete)
                on participant.UserId equals profile.UserId into profiles
            from profile in profiles.DefaultIfEmpty()
            orderby participant.JoinedAt
            select new PeerRow(participant.ConversationId, user.Username, profile != null ? profile.DisplayName : null, profile != null ? profile.ProfilePhotoUrl : null)
        ).ToListAsync();

        Dictionary<Guid, PeerInfo> peersByConversation = new Dictionary<Guid, PeerInfo>();
        foreach (PeerRow peerRow in peerRows)
        {
            bool isAlreadyKnown = peersByConversation.ContainsKey(peerRow.ConversationId);
            if (isAlreadyKnown)
                continue;
            string displayName = peerRow.DisplayName ?? peerRow.Username;
            PeerInfo peer = new PeerInfo(displayName, peerRow.PhotoUrl);
            peersByConversation[peerRow.ConversationId] = peer;
        }

        return peersByConversation;
    }

    // Unread per conversation: others' messages after join/last-read (one grouped query).
    private async Task<Dictionary<Guid, int>> LoadUnreadAsync(Guid[] conversationIds, Guid callerId)
    {
        IQueryable<TblConversationParticipant> pageMembers = _db.TblConversationParticipants.AsNoTracking()
            .Where(p => p.UserId == callerId && !p.IsDelete && p.LeftAt == null && conversationIds.Contains(p.ConversationId));

        List<UnreadRow> unreadRows = await (
            from message in _db.TblMessages.AsNoTracking()
            join member in pageMembers
                on message.ConversationId equals member.ConversationId
            where !message.IsDelete && message.SenderId != callerId && message.CreatedAt > member.JoinedAt
                && (member.LastReadMessageId == null || message.CreatedAt > _db.TblMessages.Where(last => last.Id == member.LastReadMessageId).Select(last => last.CreatedAt).FirstOrDefault())
            group message by message.ConversationId into grouped
            select new UnreadRow(grouped.Key, grouped.Count())
        ).ToListAsync();

        Dictionary<Guid, int> unreadByConversation = new Dictionary<Guid, int>();
        foreach (UnreadRow unreadRow in unreadRows)
            unreadByConversation[unreadRow.ConversationId] = unreadRow.Count;

        return unreadByConversation;
    }

    // Shapes one UI row: group profile wins for groups, else the peer (R13: UTC out for clients to localize).
    private static ChatlistItemResponse MapRow(PageRow row, Dictionary<Guid, PeerInfo> peersByConversation, Dictionary<Guid, int> unreadByConversation, DateTime nowWall)
    {
        bool isGroup = row.GroupName is not null;
        string displayName = row.GroupName ?? string.Empty;
        string? photoUrl = row.GroupPhotoUrl;
        bool hasPeer = peersByConversation.TryGetValue(row.ConversationId, out PeerInfo? peer);
        if (!isGroup && hasPeer && peer is not null)
        {
            displayName = peer.DisplayName;
            photoUrl = peer.PhotoUrl;
        }

        bool isMuted = row.MutedUntil.HasValue && row.MutedUntil.Value > nowWall;
        bool hasUnread = unreadByConversation.TryGetValue(row.ConversationId, out int unreadCount);

        LastMessageResponse? lastMessageDto = null;
        bool hasLastMessage = row.LastMessageId.HasValue;
        if (hasLastMessage)
        {
            DateTime sentAtUtc = DateTime.SpecifyKind(row.LastMessageAt!.Value, DateTimeKind.Utc);
            lastMessageDto = new LastMessageResponse
            {
                Id = row.LastMessageId!.Value,
                Content = row.LastMessageContent ?? string.Empty,
                SenderId = row.LastMessageSenderId!.Value,
                MessageType = row.LastMessageType!.Value,
                CreatedAt = sentAtUtc
            };
        }

        DateTime updatedAtUtc = DateTime.SpecifyKind(row.ActivityAt, DateTimeKind.Utc);
        ChatlistItemResponse item = new ChatlistItemResponse
        {
            ConversationId = row.ConversationId,
            Type = row.Type,
            IsGroup = isGroup,
            DisplayName = displayName,
            PhotoUrl = photoUrl,
            IsMuted = isMuted,
            IsAdmin = row.IsAdmin,
            UnreadCount = hasUnread ? unreadCount : 0,
            LastMessage = lastMessageDto,
            UpdatedAt = updatedAtUtc
        };
        return item;
    }

    // Keeps page sizes inside the contract (R6: comparisons stay out of call arguments).
    private static int ClampPageSize(int requested)
    {
        bool isTooSmall = requested < 1;
        if (isTooSmall)
            return DefaultPageSize;
        bool isTooLarge = requested > MaxPageSize;
        if (isTooLarge)
            return MaxPageSize;
        return requested;
    }

    // Treats the cursor as a UTC instant wall for without-tz comparison (null starts newest).
    private static DateTime? NormalizeCursor(DateTime? cursor)
    {
        bool hasCursor = cursor.HasValue;
        if (!hasCursor)
            return null;
        DateTime universal = cursor!.Value.ToUniversalTime();
        DateTime cursorWall = DateTime.SpecifyKind(universal, DateTimeKind.Unspecified);
        return cursorWall;
    }

    // First active peer carried through the mapping step.
    private sealed record PeerInfo(string DisplayName, string? PhotoUrl);

    // One page row of scalars only (entities in a projection ctor break Npgsql translation).
    private sealed record PageRow(
        Guid ConversationId,
        bool IsAdmin,
        DateTime? MutedUntil,
        DateTime JoinedAt,
        Guid? LastReadMessageId,
        short Type,
        DateTime ConversationCreatedAt,
        string? GroupName,
        string? GroupPhotoUrl,
        Guid? LastMessageId,
        string? LastMessageContent,
        Guid? LastMessageSenderId,
        short? LastMessageType,
        DateTime? LastMessageAt,
        DateTime ActivityAt);

    // One peer candidate row from the batched peer query.
    private sealed record PeerRow(Guid ConversationId, string Username, string? DisplayName, string? PhotoUrl);

    // One unread-count row from the batched unread query.
    private sealed record UnreadRow(Guid ConversationId, int Count);
}
