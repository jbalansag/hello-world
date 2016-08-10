using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealtyWeb.Models
{
    public class ClientInquiryMetadata
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ClientInquiryId { get; set; }
        public long PropertyEntryId { get; set; }
        public Nullable<int> UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ContactNo { get; set; }
        public string InquiryNote { get; set; }
        public string Status { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public byte[] TimeStamp { get; set; }
    }

    public class PointOfInterestMetadata
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int PoiId { get; set; }
        public long PropertyEntryId { get; set; }
        public string Name { get; set; }
        public decimal Distance { get; set; }
        public int PoiTypeId { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public byte[] TimeStamp { get; set; }
    }

    public class PointOfInterestTypeMetadata
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int PoiTypeId { get; set; }
        [Required]
        public string PoiTypeName { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public byte[] TimeStamp { get; set; }
    }

    public class PropertyMetadata
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int PropertyId { get; set; }
        public string PropertyName { get; set; }
        public string PropertyDesc { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public byte[] TimeStamp { get; set; }
    }

    public class PropertyEntryMetadata
    {
        [Key]
        [Display(Name = "Web Ref")]
        public long PropertyEntryId { get; set; }

        [Required]
        [Display(Name = "Type")]
        public int PropertyId { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string PropertyTitle { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string PropertyDetails { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal Price { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public Nullable<int> Bedrooms { get; set; }

        [DisplayFormat(DataFormatString = "{0:N0}")]
        public Nullable<int> Baths { get; set; }

        public bool AllowPet { get; set; }

        public bool HasGarden { get; set; }

        public string Address1 { get; set; }

        [Required]
        public string Address2 { get; set; }

        [Required]
        public string Address3 { get; set; }

        [Required]
        public string Address4 { get; set; }

        [Required]
        public decimal LotArea { get; set; }

        [Required]
        public decimal FloorArea { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int PropIndicatorId { get; set; }

        public IEnumerable<HttpPostedFileBase> UploadImages { get; set; }

        public List<PointOfInterest> PointOfInterests { get; set; }

        public string Status { get; set; }
        public int ContactPerson { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public byte[] TimeStamp { get; set; }
    }

    public partial class AppUserMetadata
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public Nullable<System.DateTime> LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public string LicenseNo { get; set; }
        public string ProfilePicLink { get; set; }
        [Display(Name = "Profile Description")]
        public string ProfileDetails { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
    }

    public class UserFavoriteMetadata
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserFavId { get; set; }
        public int UserId { get; set; }
        public long PropertyEntryId { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
    }

    public class ImageGalleryMetaData
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public long TranId { get; set; }
        public string ImagePath { get; set; }
        public bool Selected { get; set; }
    }

    public partial class ClientMessageMetaData
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [StringLength(50)]
        [Required]
        public string Name { get; set; }

        [DataType(DataType.EmailAddress)]
        [StringLength(100)]
        [Required]
        public string Email { get; set; }

        [StringLength(20)]
        [Required]
        public string ContactNo { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public System.DateTime CreatedDate { get; set; }
    }

    public partial class PostCardMetaData
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string ImagePath { get; set; }
        [StringLength(100)]
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public int UpdatedBy { get; set; }
    }

    public partial class SiteMetaData
    {
        public int Id { get; set; }
        public string AboutUs { get; set; }
        [StringLength(100)]
        public string ContactUs { get; set; }
        [StringLength(100)]
        public string CompanyName { get; set; }
        public string Bulletin { get; set; }
    }

    public partial class SubscriptionMetaData
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [DataType(DataType.EmailAddress)]
        [StringLength(50)]
        public string Email { get; set; }

        [StringLength(20)]
        [Required]
        public string ContactNo { get; set; }

        public bool ReceiveAlerts { get; set; }
        public System.DateTime CreatedDate { get; set; }
    }
}