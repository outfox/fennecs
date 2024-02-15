shader_type spatial;

instance uniform vec3 Albedo : source_color = vec3(.7);

void fragment() {
	ALBEDO = Albedo;	
}
