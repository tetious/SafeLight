extends Node2D


# Declare member variables here. Examples:
# var a: int = 2
# var b: String = "text"


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var poly = NavigationPolygon.new()
	var mesh : RectangleShape2D = $SolarPanel/CollisionShape2D.shape
	var width := mesh.extents.x
	var height := mesh.extents.y
	
	poly.add_outline([Vector2(0, 0),Vector2(0, height), Vector2(width, height), Vector2(width, 0)])
	poly.make_polygons_from_outlines()
	$Navigation2D.navpoly_add(poly, $SolarPanel.get_relative_transform_to_parent(self))
	$Navigation2D/TileMap.nav

	#navpoly_add($SolarPanel/NavigationPolygonInstance.navpoly, $SolarPanel/NavigationPolygonInstance.get_relative_transform_to_parent(get_parent()))
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta: float) -> void:
#	pass
