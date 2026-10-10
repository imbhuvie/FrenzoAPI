using System;
using System.Collections.Generic;
using FrenZoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FrenZoAPI.DataContext;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MstConversationType> MstConversationTypes { get; set; }

    public virtual DbSet<MstMessageStatus> MstMessageStatuses { get; set; }

    public virtual DbSet<MstMessageType> MstMessageTypes { get; set; }

    public virtual DbSet<MstUserRole> MstUserRoles { get; set; }

    public virtual DbSet<TblConversation> TblConversations { get; set; }

    public virtual DbSet<TblConversationParticipant> TblConversationParticipants { get; set; }

    public virtual DbSet<TblGroupProfile> TblGroupProfiles { get; set; }

    public virtual DbSet<TblMedium> TblMedia { get; set; }

    public virtual DbSet<TblMessage> TblMessages { get; set; }

    public virtual DbSet<TblMessageMedium> TblMessageMedia { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblUserDevice> TblUserDevices { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MstConversationType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_conversation_type_pkey");

            entity.ToTable("mst_conversation_type");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ConversationType).HasColumnName("conversation_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MstMessageStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_message_status_pkey");

            entity.ToTable("mst_message_status");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.MessageStatus).HasColumnName("message_status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MstMessageType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_message_type_pkey");

            entity.ToTable("mst_message_type");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.MessageType).HasColumnName("message_type");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MstUserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mst_user_role_pkey");

            entity.ToTable("mst_user_role");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<TblConversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_conversations_pkey");

            entity.ToTable("tbl_conversations");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TblConversations)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_created_by");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.TblConversations)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_conversation_type");
        });

        modelBuilder.Entity<TblConversationParticipant>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tbl_conversation_participants");

            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.IsAdmin)
                .HasDefaultValue(false)
                .HasColumnName("is_admin");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("joined_at");
            entity.Property(e => e.LastReadMessageId).HasColumnName("last_read_message_id");
            entity.Property(e => e.LeftAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("left_at");
            entity.Property(e => e.MutedUntil)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("muted_until");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Conversation).WithMany()
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_conversation_id");

            entity.HasOne(d => d.LastReadMessage).WithMany()
                .HasForeignKey(d => d.LastReadMessageId)
                .HasConstraintName("fnk_last_read_message_id");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_user_id");
        });

        modelBuilder.Entity<TblGroupProfile>(entity =>
        {
            entity.HasKey(e => e.ConversationId).HasName("tbl_group_profiles_pkey");

            entity.ToTable("tbl_group_profiles");

            entity.Property(e => e.ConversationId)
                .ValueGeneratedNever()
                .HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GroupName).HasColumnName("group_name");
            entity.Property(e => e.GroupPhotoUrl).HasColumnName("group_photo_url");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Conversation).WithOne(p => p.TblGroupProfile)
                .HasForeignKey<TblGroupProfile>(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_conversation_id");
        });

        modelBuilder.Entity<TblMedium>(entity =>
        {
            entity.HasKey(e => e.MediaId).HasName("tbl_media_pkey");

            entity.ToTable("tbl_media");

            entity.Property(e => e.MediaId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("media_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.MediaType).HasColumnName("media_type");
            entity.Property(e => e.Size).HasColumnName("size");
            entity.Property(e => e.ThumbnailUrl).HasColumnName("thumbnail_url");
            entity.Property(e => e.Url).HasColumnName("url");
        });

        modelBuilder.Entity<TblMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_messages_pkey");

            entity.ToTable("tbl_messages");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.MessageType).HasColumnName("message_type");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");

            entity.HasOne(d => d.Conversation).WithMany(p => p.TblMessages)
                .HasForeignKey(d => d.ConversationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_conversation_id");

            entity.HasOne(d => d.MessageTypeNavigation).WithMany(p => p.TblMessages)
                .HasForeignKey(d => d.MessageType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_message_status");

            entity.HasOne(d => d.Sender).WithMany(p => p.TblMessages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_user_id");
        });

        modelBuilder.Entity<TblMessageMedium>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tbl_message_media");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.MediaId).HasColumnName("media_id");
            entity.Property(e => e.MessageId).HasColumnName("message_id");

            entity.HasOne(d => d.Media).WithMany()
                .HasForeignKey(d => d.MediaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_media_id");

            entity.HasOne(d => d.Message).WithMany()
                .HasForeignKey(d => d.MessageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_message_id");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_users_pkey");

            entity.ToTable("tbl_users");

            entity.HasIndex(e => e.Email, "uq_email").IsUnique();

            entity.HasIndex(e => e.PhoneNumber, "uq_phone_number").IsUnique();

            entity.HasIndex(e => e.Username, "uq_username").IsUnique();

            entity.HasIndex(e => e.Email, "ux_users_email")
                .IsUnique()
                .HasFilter("(email IS NOT NULL)");

            entity.HasIndex(e => e.PhoneNumber, "ux_users_phone")
                .IsUnique()
                .HasFilter("(phone_number IS NOT NULL)");

            entity.HasIndex(e => e.Username, "ux_users_username").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        modelBuilder.Entity<TblUserDevice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tbl_user_devices_pkey");

            entity.ToTable("tbl_user_devices");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.DeviceToken).HasColumnName("device_token");
            entity.Property(e => e.DeviceType).HasColumnName("device_type");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.LastActiveAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_active_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.TblUserDevices)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_user_id");
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("user_profiles_pkey");

            entity.ToTable("user_profiles");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.Country).HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.IsDelete)
                .HasDefaultValue(false)
                .HasColumnName("is_delete");
            entity.Property(e => e.IsOnline)
                .HasDefaultValue(false)
                .HasColumnName("is_online");
            entity.Property(e => e.LastSeen)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_seen");
            entity.Property(e => e.ProfilePhotoUrl).HasColumnName("profile_photo_url");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.User).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fnk_userid");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
