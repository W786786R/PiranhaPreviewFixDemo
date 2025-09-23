
using Piranha.AttributeBuilder;
using Piranha.Models;
using Piranha.Extend;
using Piranha.Extend.Fields;

namespace PiranhaDemo.Models
{
    [PageType(Title = "Hero Page")]
    public class HeroPage : Page<HeroPage>
    {
        [Region(Title = "Hero Section")]
        public HeroRegion Hero { get; set; }
    }

    public class HeroRegion
    {
        [Field(Title = "Hero Image")]
        public ImageField HeroImage { get; set; }

        [Field(Title = "Hero Title")]
        public StringField HeroTitle { get; set; }

        [Field(Title = "Hero Body")]
        public HtmlField HeroBody { get; set; }
    }
}
