﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;

namespace SiteServer.BackgroundPages.Controls
{
    public class NavigationTree : Control
    {
        public int SiteId { get; set; }

        public string TopId { get; set; }

        public string Title { get; set; }

        public List<string> PermissionList { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            var builder = new StringBuilder();
            var tabList = TabManager.GetTabList(TopId, SiteId);
            var treeContent = BuildNavigationTree(tabList);
            if (!string.IsNullOrEmpty(treeContent))
            {
                var linkHtml = string.Empty;
                if (string.IsNullOrEmpty(TopId) && PermissionList.Contains(ConfigManager.PluginsPermissions.Management))
                {
                    linkHtml = $@"<a id=""updatePackagesLink"" href=""{PageUtils.GetAdminUrl(PageUtils.Combine("plugins/manage.cshtml?pageType=4"))}"" onclick=""closeMenu()"" class=""badge badge-warning"" style=""display: none"" target=""right""></a>";
                }
                builder.Append($@"<li class=""text-muted menu-title"">{Title}{linkHtml}</li>{treeContent}");
            }
            writer.Write(builder);
        }

        private string BuildNavigationTree(List<Tab> tabs)
        {
            if (tabs == null || tabs.Count == 0) return string.Empty;

            var builder = new StringBuilder();

            foreach (var parent in tabs)
            {
                if (!TabManager.IsValid(parent, PermissionList)) continue;

                var childBuilder = new StringBuilder();
                if (parent.Children != null && parent.Children.Length > 0)
                {
                    var tabCollection = new TabCollection(parent.Children);
                    if (tabCollection.Tabs != null && tabCollection.Tabs.Length > 0)
                    {
                        foreach (var childTab in tabCollection.Tabs)
                        {
                            if (!TabManager.IsValid(childTab, PermissionList)) continue;

                            var href = childTab.Href;
                            if (!PageUtils.IsAbsoluteUrl(href))
                            {
                                href = PageUtils.AddQueryString(href,
                                    new NameValueCollection { { "siteId", SiteId.ToString() } });
                            }

                            if (childTab.HasHref)
                            {
                                childBuilder.Append($@"
<li>
    <a href=""{PageUtils.GetLoadingUrl(href)}"" onclick=""closeMenu()"" target=""{(string.IsNullOrEmpty(childTab.Target) ? "right" : childTab.Target)}"">
        <i class=""{childTab.IconClass}""></i>
        {childTab.Text}
    </a>
</li>");
                            }
                            else
                            {
                                childBuilder.Append($@"
<li>
    <a href=""javascript:;"">
        <i class=""{childTab.IconClass}""></i>
        {childTab.Text}
    </a>
</li>");
                            }
                            
                        }
                    }
                }

                if (childBuilder.Length > 0)
                {
                    builder.Append($@"
<li class=""has_sub"">
    <a href=""javascript:;"" class=""waves-effect waves-primary {(parent.Selected ? "subdrop" : "")}"" target=""{(string.IsNullOrEmpty(parent.Target) ? "right" : parent.Target)}"">
        <i class=""{parent.IconClass ?? "ion-star"}""></i>
        <span> {parent.Text} </span>
        <span class=""menu-arrow""></span>
    </a>
    <ul class=""list-unstyled"" style=""display: {(parent.Selected ? "block" : "none")};"">
        {childBuilder}
    </ul>
</li>
");
                }
                else
                {
                    var href = parent.Href;
                    if (!PageUtils.IsAbsoluteUrl(href))
                    {
                        href = PageUtils.AddQueryString(href,
                            new NameValueCollection {{"siteId", SiteId.ToString()}});
                    }

                    if (parent.HasHref)
                    {
                        builder.Append($@"
<li class=""has_sub"">
    <a href=""{PageUtils.GetLoadingUrl(href)}"" onclick=""closeMenu()"" class=""waves-effect waves-primary {(parent.Selected ? "subdrop" : "")}"" target=""{(string.IsNullOrEmpty(parent.Target) ? "right" : parent.Target)}"">
        <i class=""{parent.IconClass ?? "ion-star"}""></i>
        <span> {parent.Text} </span>
    </a>
</li>");
                    }
                    else
                    {
                        builder.Append($@"
<li class=""has_sub"">
    <a href=""javascript:;"" class=""waves-effect waves-primary {(parent.Selected ? "subdrop" : "")}"">
        <i class=""{parent.IconClass ?? "ion-star"}""></i>
        <span> {parent.Text} </span>
    </a>
</li>");
                    }
                    
                }
            }

            return builder.ToString();
        }
    }
}
