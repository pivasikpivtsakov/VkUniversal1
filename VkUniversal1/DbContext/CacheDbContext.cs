using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VkUniversal1.DbContext
{
    public sealed class CacheDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<VkInbox> Inboxes { get; set; }
        public DbSet<VkPeer> VkPeers { get; set; }
        public DbSet<VkMessage> VkMessages { get; set; }
        public DbSet<SessionInfo> SessionInfo { get; set; }

        public CacheDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "cache.db");
            options.UseSqlite("Filename = " + dbPath);
        }
    }
}