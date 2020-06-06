using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using Windows.UI.Xaml.Media;
using Microsoft.EntityFrameworkCore;

namespace VkUniversal1
{
    public sealed class CacheDbContext : DbContext
    {
        public DbSet<VkInbox> Inboxes { get; set; }
        public DbSet<VkPeer> VkPeers { get; set; }
        public DbSet<VkMessage> VkMessages { get; set; }
        public DbSet<SessionInfo> SessionInfo { get; set; }

        public CacheDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VkMessage>()
                .HasOne(p => p.VkPeer)
                .WithMany(p => p.VkMessages);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "cache.db");
            options.UseSqlite("Filename = " + dbPath);
        }
    }

    public class VkPeer
    {
        [Key] public long PeerId { get; set; }

        public string ReadableName { get; set; }
        public byte[] Avatar { get; set; }
        public string LastMessage { get; set; }
        public List<VkMessage> VkMessages { get; set; }
    }

    public class VkMessage
    {
        [Key] public int DummyKey { get; set; }
        public long PeerId { get; set; }
        public long FromId { get; set; }
        public long ConversationMessageId { get; set; }
        public long InboxId { get; set; }
        public string Text { get; set; }
        //todo attachments, fwd_messages

        public VkPeer VkPeer { get; set; }
    }

    public class SessionInfo
    {
        [Key] public int DummyKey { get; set; }
        public string AccessToken { get; set; }
        public long UserId { get; set; }
        public string ReadableName { get; set; }
    }

    public enum InboxType
    {
        User, Group
    }
    
    public class VkInbox
    {
        [Key] public long Id { get; set; }
        public InboxType Type { get; set; }
        public byte[] Avatar { get; set; }
        public string ReadableName { get; set; }
        public List<VkMessage> VkMessages { get; set; }
    }
}