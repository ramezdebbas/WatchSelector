using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace EcommFashion.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : EcommFashion.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get { return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
                        "Nivax Data");

            var group1 = new SampleDataGroup("Group-1",
                    "Best Watches for Men",
                    "Group Subtitle: 1",
                    "Assets/DarkGray.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "Kenneth Cole Men's Watches",
                    "",
                    "Assets/HubPage/HubPage1.png",
                    "Kenneth Cole started his career designing women's shoes and soon branched out to include women's fashion, men's fashion and ultimately accessories such as jewelry and watches. Kenneth Cole men's watches are beautifully designed and often can be mistaken for higher end brands such as Omega for people that aren't avid about men's watches. Generally quite affordable for the most part, you can find a lot of Kenneth Cole watches under $200 that are made with great materials and are classy and stylish. See for yourself below for some designs from one of the best watch brands for men!",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                   "Tissot Men's Watches",
                    "",
                    "Assets/HubPage/HubPage2.png",
                    "Normally Tissot is considered a luxury watch maker, originating from Switzerland in 1853. However, they do make plenty of affordable watches that are under $500, which makes them one of the mens watch brands to consider. So while $500 isn't a small sum of money, you are getting a design and materials that are normally only found on luxury brands. Watches can range from a couple hundred dollars to a couple thousand dollars, so the range is quite large. You may be interested to know that Tissot is the official timekeeper for many international competitions, including ice hockey, cycling and motorcycle racing. This should definitely tell you about the quality of their timepieces and their watches. Take one look at their design and you will understand why they are so desirable and highly praised!",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "Seiko Men's Watches",
                    "",
                    "Assets/HubPage/HubPage3.png",
                    "Seiko was founded in the 19th century, specifically in 1881 in Toyko, Japan. Did you know that the word Seiko means exquisite in the Japanese language? Although they have been around since 1881, the first time Seiko made watches was in 1924, which gives them plenty of practice and experience in developing the perfect watches for people around the world. There are two types of watches that Seiko mainly makes, quarts type watch and mechanical type watches and the price can range from $50 USD to $500,000 USD! Although, you can guess that they sell more $50 ones than the more expensive variety. Some of the ones listed below are a good quality watch while maintaining a good value as one of the top watch brands for men available.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-4",
                    "Fossil Men's Watches",
                    "",
                    "Assets/HubPage/HubPage4.png",
                    "Fossil is an American brand that specializes in designing and manufacturing clothing and accessories that are aimed at the young adult market. Founded in 184, their aim is to provide them with recreational goods that are desirable and feel vintage at an affordable price. Many of the pieces that Fossil produces are considered as collectible pieces and are sometimes found to be based on items that are pop-culture related. In either case, their watches are among some of the more popular watch brands for men and are a perfect gift for any occasion.",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-5",
                    "Citizen Men's Watches",
                    "",
                    "Assets/HubPage/HubPage5.png",
                    "Citizen is known for incorporating some cool technology into its watches. As a good watch brand for men, you can find Citizen watches that have technology such as Eco-Drive and Atomic Timekeeping. Eco-Drive Technology is where Citizen incorporates a solar panel underneath the face of the watch and allows the battery to recharge with natural sunlight or even indoor lighting. With Atomic Timekeeping technology, you are able to sync the time to your phone through the clock radio waves in North America, Europe and Japan. This will always ensure that your time keeping is correct and won't throw you off. Most importantly though, Citizen has a wide variety of wrist watch styles that will be sure to suit any man's tastes, no matter if they are looking for dressy, sporty or luxurious looking. Take a look for yourself and decide!",
                    ITEM_CONTENT,
                    group1));
            group1.Items.Add(new SampleDataItem("Group-1-Item-6",
                    "Victorinox Men's Watches",
                    "",
                    "Assets/HubPage/HubPage6.png",
                    "Victorinox is best known for being the maker of Swiss Army Knives, however, they make a lot more than the iconic functional pocket knives. Some of the watches that are made by Victorinox are one of the most durable and well constructed watches ever made and available on the market at an affordable price. As with their Swiss Army Knives, the watches themselves are usually loaded with features that are shown on the watch face that looks like there are plenty of things going on. Although it might look a bit busy, it definitely falls in line with the company's products and branding strategy. If you are a man wearing one of these watches, you will definitely have the rugged feel and aura surrounding you, regardless if you wield a Swiss Army Knife or not.",
                    ITEM_CONTENT,
                    group1));
            this.AllGroups.Add(group1);

             var group2 = new SampleDataGroup("Group-2",
                    "Best Watches for Women",
                    "Group Subtitle: 2",
                    "Assets/LightGray.png",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
					"Chanel Women's H0970 J12 White Ceramic Bracelet Watch",
					"",
                    "Assets/HubPage/HubPage7.png",
                    "This beautiful Chanel ceramic watch is number 10 in the top 10 women's luxury watches. This Chanel J12 watch is one of the most popular Chanel watches ever made. It is worn and adored by many high profile celebrities including Sharon Osborne and Fearne Cotton. It is an expensive watch but you get what you pay for. Amazon currently have a small discount running on this watch which makes it a little cheaper than it's normal RRP. The watch consists of a white ceramic case with a white ceramic bracelet. Ceramic watches are still very much ruling the watch fashion trends. I think white watches are particularly great for summer and would look great against a suntan. Additionally I love the contrast of a white watch against a black or navy dress. It draws even more attention to the watch, which is exactly what you will want if you purchase this white luxury women's watch.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "TAG Heuer Women's Aquaracer Diamond Two-Tone Mother-of-Pearl Dial Watch",
                    "",
                    "Assets/HubPage/HubPage8.png",
                    "This TAG Heuer Women's WAF1450.BB0825 Aquaracer Diamond Two-Tone Mother-of-Pearl Dial Watch is number 8 in the top 10 women's luxury watches countdown. Not surprisingly it is one of of several featured in the top 10. This women's luxury watch will last and last. It is guaranteed to get you attention day and night. Best of all Amazon have almost a third of RRP making it more affordable. This beautiful Tag Heuer two tone watch will make a great gift for women who wear both gold and silver jewelry. The 45 diamonds around the watch face give it that bit sparkle which will make it suitable for casual and evening wear. One of my favorite watches in the top 10 women's luxury watches and one on my wish list!!",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                     "Baume & Mercier Women's 8666 Diamant Swiss Diamond Watch",
                    "",
                    "Assets/HubPage/HubPage9.png",
                    "This Baume & Mercier Women's Diamond Watch is number 9 in the top 10 women's luxury watches of 2012. Currently the watch is well discounted with 60% off RRP! Sparkly diamonds, comfortable bracelet and nice mother of pearl face is what one recent reviewer has to say about this stunning watch.",
                    ITEM_CONTENT,
                    group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-4",
                    "TAG Heuer Women's WV1411.BA0793 Carrera Diamond Watch",
                   "",
                   "Assets/HubPage/HubPage10.png",
                   "This TAG Heuer Women's Carrera Diamond Watch makes an entry at number 6 in the top 10 women's luxury watches. It has an Amazon rating of 4.7/5 after 3 reviews and is a stunning piece. It is a simple watch in stainless steel and accessorized with diamonds which will catch the light and sparkle.",
                   ITEM_CONTENT,
                   group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-5",
                    "Cartier Women's W51008Q3 Tank Francaise Stainless Steel Watch",
                   "",
                   "Assets/HubPage/HubPage11.png",
                   "The Cartier Women's W51008Q3 Tank Francaise Stainless Steel Watch makes an entry in at number 5 in the top 10 women's luxury watches.",
                   ITEM_CONTENT,
                   group2));
            group2.Items.Add(new SampleDataItem("Group-2-Item-6",
                    "TAG Heuer Women's WAH1214BA0859 Formula 1 Ceramic Watch",
                   "",
                   "Assets/HubPage/HubPage12.png",
                   "This TAG Heuer Women's WAH1214BA0859 Formula 1 Ceramic Watch is number 4 in the top 10 women's luxury watches. Even better is the fact that this watch has a discount of 33% as we speak.",
                   ITEM_CONTENT,
                   group2));
            this.AllGroups.Add(group2); 


        }
    }
}
