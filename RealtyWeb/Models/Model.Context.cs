﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace RealtyWeb.Models
{
    public partial class RealtyWebContext : DbContext
    {
        public RealtyWebContext()
            : base("name=RealtyWebContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Barangay> Barangays { get; set; }
        public DbSet<CityMunicipality> CityMunicipalities { get; set; }
        public DbSet<Constant> Constants { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<LocationPath> LocationPaths { get; set; }
        public DbSet<PointOfInterestType> PointOfInterestTypes { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyIndicator> PropertyIndicators { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<ClientInquiry> ClientInquiries { get; set; }
        public DbSet<ImageGallery> ImageGalleries { get; set; }
        public DbSet<PointOfInterest> PointOfInterests { get; set; }
        public DbSet<PropertyEntry> PropertyEntries { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<UserClaim> UserClaims { get; set; }
        public DbSet<UserLogin> UserLogins { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ClientMessage> ClientMessages { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<PostCard> PostCards { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
    }
}
