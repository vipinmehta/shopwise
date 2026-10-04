namespace ECommerce.Infrastructure.Seed;

/// <summary>
/// Real product photographs (Unsplash, free licence) used by the demo seed. Each entry is
/// "photoId|colour|style". Colour and style were taken from each photo's own description, keeping
/// only photos where exactly one colour is named and it belongs to the garment/shoe itself.
/// Images are hotlinked from images.unsplash.com.
/// </summary>
public static class PhotoCatalog
{
    public static string Url(string photoId) =>
        $"https://images.unsplash.com/photo-{photoId}?auto=format&fit=crop&w=800&h=1000&q=80";

    public static readonly Dictionary<string, string[]> Photos = new()
    {
        ["men:tee"] = new[]
        {
            "1583743814966-8936f5b7be1a|black|Crew Neck Tee",
            "1581655353564-df123a1eb820|white|Crew Neck Tee",
            "1622492885933-663029fce455|orange|Crew Neck Tee",
            "1621951753163-ee63e7fc147e|blue|Crew Neck Tee",
            "1624373607006-348f61ea2d76|red|Crew Neck Tee",
            "1581803118532-d6fc1600c77e|maroon|Crew Neck Tee",
            "1622383128901-aa2c3399c75c|pink|Crew Neck Tee",
            "1600466519238-7ee2895f24fd|grey|Crew Neck Tee",
            "1584009083449-ed6a9d90aea9|brown|Crew Neck Tee",
            "1641319906023-a6521d213650|yellow|Polo T-Shirt",
            "1618453292459-53424b66bb6a|black|Crew Neck Tee",
            "1521572163474-6864f9cf17ab|white|Crew Neck Tee"
        },
        ["women:tee"] = new[]
        {
            "1618354691373-d851c5c3a990|black|Crew Neck Tee",
            "1618677603544-51162346e165|white|Crew Neck Tee",
            "1601327871285-b4fc0b9bbf5a|yellow|Crew Neck Tee",
            "1775234576241-54cbf57ffd74|pink|Classic Tee",
            "1604780380443-85a7f59f2f1b|brown|Everyday Tee",
            "1562767173-28e3baa9737d|grey|Essential Tee",
            "1619227422405-7c95138a2c9a|red|V-Neck Tee",
            "1587590876544-978dfa7342d1|orange|Crew Neck Tee",
            "1759572095329-1dcf9522762b|green|Relaxed Fit Tee",
            "1599255068390-206e0d068539|black|Crew Neck Tee",
            "1543302764-f18c04c40e47|yellow|Classic Tee"
        },
        ["men:jeans"] = new[]
        {
            "1714143136367-7bb68f3f0669|blue|Classic Fit Jeans",
            "1587028971442-aa483a05136a|black|Straight Fit Jeans",
            "1520516668418-3661351e2e54|grey|Distressed Jeans",
            "1565416700374-d0efa117d162|blue|Slim Fit Jeans",
            "1718252540585-499e23ef57a2|black|Tapered Jeans",
            "1555689502-c4b22d76c56f|blue|Classic Fit Jeans",
            "1505940145182-6718fd38efb1|black|Distressed Jeans",
            "1542574621-e088a4464f7e|blue|Straight Fit Jeans",
            "1640336437301-8368b53861ab|blue|Slim Fit Jeans",
            "1547410701-73b5a0ada51d|blue|Tapered Jeans",
            "1565084888279-aca607ecce0c|blue|Classic Fit Jeans"
        },
        ["women:jeans"] = new[]
        {
            "1516271099866-de31ba93ee4b|black|Distressed Jeans",
            "1715532846484-1b10ddf694d0|blue|Straight Fit Jeans",
            "1468608312765-da182c3cac11|black|Slim Fit Jeans",
            "1541099649105-f69ad21f3246|blue|Tapered Jeans",
            "1578693082747-50c396cacd81|blue|Classic Fit Jeans",
            "1577210897949-1f56f943bf82|blue|Straight Fit Jeans",
            "1762752872078-982cd2b499a4|blue|Slim Fit Jeans",
            "1576995853123-5a10305d93c0|blue|Tapered Jeans"
        },
        ["men:shoes"] = new[]
        {
            "1614253429340-98120bd6d753|brown|Dress Shoe",
            "1668069226492-508742b03147|black|Casual Shoe",
            "1770198408387-7f45e5d6c056|blue|Leather Shoe",
            "1517389274750-a758d503b69e|beige|Chukka Boot",
            "1662465829657-3d1251ba943f|red|Formal Shoe",
            "1603191659812-ee978eeeef76|brown|Dress Shoe",
            "1783455042943-1a690e6dd46b|black|Casual Shoe",
            "1790634271193-ab9fea24904a|blue|Oxford Shoe",
            "1533867617858-e7b97e060509|brown|Loafer",
            "1657863598793-7a46461dcd4d|black|Leather Shoe"
        },
        ["women:shoes"] = new[]
        {
            "1596703263926-eb0762ee17e4|black|High Heel",
            "1621996659490-3275b4d0d951|brown|High Heel",
            "1566041254940-fe7a3b50257e|white|Pump Heel",
            "1670607231621-c00fd76d2387|red|High Heel",
            "1788773396956-086888993825|blue|Dress Shoe",
            "1784821926007-5402477b0669|gold|High Heel",
            "1782232460628-ab38d15bd4af|beige|High Heel",
            "1654056794740-8899e0aa9039|yellow|Casual Shoe",
            "1565645692663-914d6c14db75|green|Pump Heel",
            "1553028826-7c442e636161|pink|Pump Heel",
            "1590099033615-be195f8d575c|grey|Strappy Sandal",
            "1562273138-f46be4ebdf33|maroon|Wedge Sandal"
        },
        ["men:sneakers"] = new[]
        {
            "1544441892-794166f1e3be|white|Low-Top Sneaker",
            "1632497775897-815042a13216|black|Casual Sneaker",
            "1778361994945-8224bdb9f2a2|brown|High-Top Sneaker",
            "1540075238217-985a5bbfcc90|blue|Everyday Sneaker",
            "1544441892-83af2e53ea48|grey|Low-Top Sneaker",
            "1626016936532-46a361b138fc|orange|Lifestyle Sneaker",
            "1683563313829-923466fc495d|red|Street Sneaker",
            "1657196085088-6ec691525791|yellow|Casual Sneaker",
            "1771710974003-36707a93f37a|white|Everyday Sneaker",
            "1574020462714-5451391cc336|black|Lifestyle Sneaker",
            "1778361994926-2bbfe1d553dd|brown|High-Top Sneaker",
            "1680821440051-0d7ec9e5b325|blue|Street Sneaker"
        },
        ["women:sneakers"] = new[]
        {
            "1689357642277-65228ee23680|white|Casual Sneaker",
            "1742392787511-8158c243b772|pink|Everyday Sneaker",
            "1743100619786-ed873fb1eaf5|beige|Lifestyle Sneaker",
            "1565121354173-a5e543daa4ad|teal|Running Shoe",
            "1662182670987-d38f3b0a00c6|brown|Street Sneaker",
            "1542665889681-5fd1eb5c9f5b|red|Low-Top Sneaker",
            "1554068420-e7c9ae1fc504|black|Low-Top Sneaker",
            "1559504344-33abd17324d5|yellow|Casual Sneaker",
            "1599670998937-441a3a74b2f1|white|Low-Top Sneaker",
            "1675108658061-61c98ee47ced|pink|Everyday Sneaker",
            "1743100619209-0f3695fa4c92|beige|Lifestyle Sneaker",
            "1554192832-58cc7b19c7ea|teal|High-Top Sneaker"
        }
    };
}
