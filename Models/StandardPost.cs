using Piranha.AttributeBuilder;
using Piranha.Models;

namespace PiranhaDemo.Models;

[PostType(Title = "Standard post")]
public class StandardPost  : Post<StandardPost>
{
}
