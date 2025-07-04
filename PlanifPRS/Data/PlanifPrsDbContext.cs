﻿using Microsoft.EntityFrameworkCore;
using PlanifPRS.Models;

namespace PlanifPRS.Data
{
    public class PlanifPrsDbContext : DbContext
    {
        public PlanifPrsDbContext(DbContextOptions<PlanifPrsDbContext> options) : base(options) { }

        public DbSet<Prs> Prs { get; set; }
        public DbSet<ChecklistStandard> ChecklistsStandards { get; set; }
        public DbSet<PrsJalon> PrsJalons { get; set; }
        public DbSet<Ligne> Lignes { get; set; }
        public DbSet<PrsFamille> PrsFamilles { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<JalonUtilisateur> JalonUtilisateurs { get; set; }
        public DbSet<Secteur> Secteurs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ⭐ Table PRS_Famille - CORRECTION ICI
            modelBuilder.Entity<PrsFamille>(entity =>
            {
                entity.ToTable("PRS_Famille"); // ✅ Nom de table corrigé avec underscore
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Libelle)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(e => e.CouleurHex)
                      .HasMaxLength(7); // exemple : "#007bff"
            });

            // Configuration pour la table Secteur
            modelBuilder.Entity<Secteur>(entity =>
            {
                entity.ToTable("Secteur", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.IdTypeSecteur).HasColumnName("idTypeSecteur");
                entity.Property(e => e.Nom).HasColumnName("nom").IsRequired();
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Activation).HasColumnName("activation");
                entity.Property(e => e.DoNotDelete).HasColumnName("doNotDelete");
                entity.Property(e => e.DateCreated).HasColumnName("dateCreated");
                entity.Property(e => e.DateModified).HasColumnName("dateModified");
                entity.Property(e => e.DateDeleted).HasColumnName("dateDeleted");
            });

            // Configuration pour la table Ligne
            modelBuilder.Entity<Ligne>(entity =>
            {
                entity.ToTable("Lignes", "dbo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(255);
                entity.Property(e => e.IdSecteur).HasColumnName("idSecteur");
                entity.Property(e => e.Description).HasMaxLength(100).HasColumnName("description");
                entity.Property(e => e.Activation).HasColumnName("activation");
                entity.Property(e => e.DoNotDelete).HasColumnName("doNotDelete");
                entity.Property(e => e.DateCreated).HasColumnName("dateCreated");
                entity.Property(e => e.DateModified).HasColumnName("dateModified");
                entity.Property(e => e.DateDeleted).HasColumnName("dateDeleted");

                // Relation Ligne → Secteur
                entity.HasOne(l => l.Secteur)
                      .WithMany()
                      .HasForeignKey(l => l.IdSecteur)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ⭐ Relation PRS → PrsFamille - AJOUT DE LA RELATION
            modelBuilder.Entity<Prs>()
                .HasOne(p => p.Famille)
                .WithMany()
                .HasForeignKey(p => p.FamilleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation PRS → Ligne
            modelBuilder.Entity<Prs>()
                .HasOne(p => p.Ligne)
                .WithMany(l => l.PRSs)
                .HasForeignKey(p => p.LigneId)
                .OnDelete(DeleteBehavior.Restrict);

            // Clé alternative pour LoginWindows dans Utilisateur
            modelBuilder.Entity<Utilisateur>()
                .HasAlternateKey(u => u.LoginWindows);

            // ChecklistStandard → Utilisateur via LoginWindows
            modelBuilder.Entity<ChecklistStandard>()
                .HasOne(cs => cs.Utilisateur)
                .WithMany()
                .HasForeignKey(cs => cs.CreePar)
                .HasPrincipalKey(u => u.LoginWindows)
                .OnDelete(DeleteBehavior.Restrict);

            // Mapping PRS_Jalons
            modelBuilder.Entity<PrsJalon>(entity =>
            {
                entity.ToTable("PRS_Jalons");
                entity.HasKey(e => e.Id);

                entity.HasOne(j => j.Prs)
                      .WithMany(p => p.Jalons)
                      .HasForeignKey(j => j.PRSId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Mapping JalonUtilisateurs (clé composite)
            modelBuilder.Entity<JalonUtilisateur>(entity =>
            {
                entity.ToTable("JalonUtilisateurs");

                entity.HasKey(ju => new { ju.JalonId, ju.UtilisateurId });

                entity.HasOne(ju => ju.PrsJalon)
                      .WithMany(j => j.JalonUtilisateurs)
                      .HasForeignKey(ju => ju.JalonId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ju => ju.Utilisateur)
                      .WithMany(u => u.JalonUtilisateurs)
                      .HasForeignKey(ju => ju.UtilisateurId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}