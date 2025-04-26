
namespace Basket.Data.JsonConverters;
public class ShoppingCartConverter : JsonConverter<ShoppingCart>
{
    public override ShoppingCart? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonDocument = JsonDocument.ParseValue(ref reader);
        var roolElement = jsonDocument.RootElement;

        var id = roolElement.GetProperty("id").GetGuid();
        var userName = roolElement.GetProperty("userName").GetString();
        var itemsElement = roolElement.GetProperty("items");

        var shoppingCart = ShoppingCart.Create(id, userName);

        var items = itemsElement.Deserialize<List<ShoppingCart>>(options);
        if (items != null)
        {
            var itemsField = typeof(ShoppingCart).GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
            itemsField?.SetValue(shoppingCart, items);
        }

        return shoppingCart;
    }

    public override void Write(Utf8JsonWriter writer, ShoppingCart value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("id", value.Id.ToString());
        writer.WriteString("userName", value.UserName);

        writer.WritePropertyName("items");
        JsonSerializer.Serialize(writer, value.Items, options);

        writer.WriteEndObject();
    }
}
