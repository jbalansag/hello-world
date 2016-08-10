using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RealtyWeb.Models
{
    [MetadataType(typeof(ClientInquiryMetadata))]
    public partial class ClientInquiry {}

    [MetadataType(typeof(PointOfInterestMetadata))]
    public partial class PointOfInterest {}

    [MetadataType(typeof(PointOfInterestTypeMetadata))]
    public partial class PointOfInterestType {}

    [MetadataType(typeof(PropertyMetadata))]
    public partial class Property {}

    [MetadataType(typeof(PropertyEntryMetadata))]
    public partial class PropertyEntry 
    {
        public IEnumerable<HttpPostedFileBase> UploadImages { get; set; }
        public List<PointOfInterest> PointOfInterests { get; set; }
    }

    [MetadataType(typeof(AppUserMetadata))]
    public partial class AppUser { }

    [MetadataType(typeof(UserFavoriteMetadata))]
    public partial class UserFavorite {}

    [MetadataType(typeof(ImageGalleryMetaData))]
    public partial class ImageGallery {
        public bool Selected { get; set; }
    }

    [MetadataType(typeof(ClientMessageMetaData))]
    public partial class ClientMessage
    {}

    [MetadataType(typeof(PostCardMetaData))]
    public partial class PostCard
    {}

    [MetadataType(typeof(SiteMetaData))]
    public partial class Site
    {}

    [MetadataType(typeof(SubscriptionMetaData))]
    public partial class Subscription
    { }
}