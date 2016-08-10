using System.Web;
using System.Web.Optimization;

namespace RealtyWeb
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/scripts").Include(
                        "~/Scripts/jquery-1.8.2.min.js",
                        "~/Scripts/jquery.mousewheel-3.0.6.pack.js",
                        "~/Scripts/jquery.fancybox.js",
                        "~/Scripts/jquery.unobtrusive-ajax.min.js",
                        "~/Scripts/select2.js",
                        "~/Scripts/typeahead.mvc.model.js",
                        "~/App/Scripts/infiniteScroll.js",
                        "~/Scripts/jquery.cookie.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/details").Include("~/Scripts/jquery-1.9.1.min.js",
                                                                     "~/Scripts/jssor.slider.mini.js",
                                                                     "~/Scripts/jquery.unobtrusive-ajax.min.js",
                                                                     "~/Scripts/jquery.cookie.js"));

            bundles.Add(new ScriptBundle("~/bundles/inquiries").Include("~/Scripts/moment.min.js",
                                                                     "~/Scripts/bootstrap.min.js",
                                                                     "~/Scripts/bootstrap-datetimepicker.js"));

            bundles.Add(new ScriptBundle("~/bundles/typeahead").Include("~/Scripts/typeahead.bundle*"));
            bundles.Add(new ScriptBundle("~/bundles/typeahead-bloodhound").Include("~/Scripts/bloodhound*"));
            bundles.Add(new ScriptBundle("~/bundles/typeahead-jquery").Include("~/Scripts/typeahead.jquery*"));

            bundles.Add(new StyleBundle("~/Content/inquiries").Include("~/Content/bootstrap-datetimepicker.css"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                     "~/Content/bootstrap.css",
                     "~/Content/Site.css",
                     "~/Content/font-awesome.css",
                     "~/Content/jquery.fancybox.css",
                     "~/Content/typeahead.css",
                     "~/Content/PagedList.css",
                     "~/Content/select2.css",
                     "~/Content/PagedList.css"));

            BundleTable.EnableOptimizations = true;
        }
    }
}
