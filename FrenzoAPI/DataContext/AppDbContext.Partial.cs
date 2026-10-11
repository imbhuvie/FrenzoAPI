using FrenZoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FrenZoAPI.DataContext;

// Scaffold-safe model tweaks: this file is never touched by `dotnet ef dbcontext scaffold --force`
// (that regenerates AppDbContext.cs only), so keys configured here survive re-scaffolding.
// The tables below have NO database primary key; these HasKey calls give EF change tracking
// (required for Add/SaveChanges) without creating any DB constraint. Duplicates stay app-guarded.
public partial class AppDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Join table for chat membership; pair uniqueness is enforced by service checks + transaction.
        modelBuilder.Entity<TblConversationParticipant>()
            .HasKey(p => new { p.ConversationId, p.UserId });

        // Join table for message attachments; same treatment for the future send-message endpoint.
        modelBuilder.Entity<TblMessageMedium>()
            .HasKey(m => new { m.MessageId, m.MediaId });
    }
}
