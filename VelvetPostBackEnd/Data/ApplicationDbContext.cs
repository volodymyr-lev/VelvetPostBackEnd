using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using VelvetPostBackEnd.Models;

namespace VelvetPostBackEnd.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Client> Clients { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Terminal> Terminals { get; set; }
    public DbSet<PostOffice> PostOffices { get; set; }
    public DbSet<PostOfficeEmployee> PostOfficeEmployees { get; set; }
    public DbSet<TerminalEmployee> TerminalEmployees { get; set; }
    public DbSet<Parcel> Parcels { get; set; }
    public DbSet<Shipment> Shipments { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {   }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Client config
        builder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(c => c.LastName).IsRequired().HasMaxLength(100);
            entity.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(100);
            entity.HasIndex(c => c.PhoneNumber).IsUnique();
            entity.Property(c => c.Email).HasMaxLength(100);
            entity.Property(c => c.Address).HasMaxLength(100);

            entity.HasOne(c => c.ApplicationUser)
                .WithOne(u => u.Client)
                .HasForeignKey<Client>(c => c.ApplicationUserId)
                .IsRequired(false);
        });



        // Employee config
        builder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Position).IsRequired();
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.StartDate).IsRequired();

            // ApplicationUser rel (opt)
            entity.HasOne(e => e.ApplicationUser)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.ApplicationUserId)
                .IsRequired(false);
        });



        // Terminal config
        builder.Entity<Terminal>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Address).IsRequired().HasMaxLength(255);
            entity.Property(t => t.City).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Type).IsRequired();
        });



        // PostOffice config
        builder.Entity<PostOffice>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Address).IsRequired().HasMaxLength(255);
            entity.Property(p => p.City).IsRequired().HasMaxLength(100);
            entity.Property(p => p.PhoneNumber).IsRequired().HasMaxLength(20);

            // Terminal rel
            entity.HasOne(p => p.Terminal)
                .WithMany(t => t.PostOffices)
                .HasForeignKey(p => p.TerminalId)
                .IsRequired(false);
        });



        // PostOfficeEmployee config
        builder.Entity<PostOfficeEmployee>(entity =>
        {
            entity.HasKey(p => p.Id);

            // Employee rel
            entity.HasOne(p => p.Employee)
                .WithOne(e => e.PostOfficeEmployee)
                .HasForeignKey<PostOfficeEmployee>(p => p.EmployeeId);

            // PostOffice rel
            entity.HasOne(p => p.PostOffice)
                .WithMany(po => po.PostOfficeEmployees)
                .HasForeignKey(p => p.PostOfficeId);
        });



        // TerminalEmployee config
        builder.Entity<TerminalEmployee>(entity =>
        {
            entity.HasKey(t => t.Id);

            // Employee rel
            entity.HasOne(t => t.Employee)
                .WithOne(e => e.TerminalEmployee)
                .HasForeignKey<TerminalEmployee>(t => t.EmployeeId);

            // Terminal rel
            entity.HasOne(t => t.Terminal)
                .WithMany(term => term.TerminalEmployees)
                .HasForeignKey(t => t.TerminalId);
        });



        // Parcel config
        builder.Entity<Parcel>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Weight).IsRequired().HasColumnType("decimal(5,2)");
            entity.Property(p => p.Type).IsRequired();
        });



        // Shipment config
        builder.Entity<Shipment>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.CreatedAt).IsRequired();
            entity.Property(s => s.Status).IsRequired();

            // Client Shipper rel
            entity.HasOne(s => s.Sender)
                .WithMany(c => c.SentShipments)
                .HasForeignKey(s => s.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Client Receiver rel
            entity.HasOne(s => s.Receiver)
                .WithMany(c => c.ReceivedShipments)
                .HasForeignKey(s => s.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sender PostOffice rel
            entity.HasOne(s => s.FromPostOffice)
                .WithMany(p => p.OutgoingShipments)
                .HasForeignKey(s => s.FromPostOfficeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Receiver PostOffice rel
            entity.HasOne(s => s.ToPostOffice)
                .WithMany(p => p.IncomingShipments)
                .HasForeignKey(s => s.ToPostOfficeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Parcel rel
            entity.HasOne(s => s.Parcel)
                .WithOne(p => p.Shipment)
                .HasForeignKey<Shipment>(s => s.ParcelId)
                .IsRequired(false);
        });
    }
}
