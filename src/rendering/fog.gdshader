shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform int light_status[1024];
uniform int board_height = 12;
uniform int board_width = 12;

void fragment() {
	//board_height = 12;
	//board_width = 12;
	//vec4 overlay_sample = texture(overlay, UV);
	vec4 screen_sample = texture(screen_texture, SCREEN_UV);
	//COLOR.rgb *= 1.5;
	int x = int(UV.x*float(board_width));
	int y = int(UV.y*float(board_height));
	float ratio = 0.18;
	int index = x+y*board_width;
	if(light_status[index]==1)
		ratio = 1.;
	else if(light_status[index]==2)
		ratio = .5;
	else {
		if(x>0 && light_status[index-1]==1) ratio = .4;
		else if(y>0 && light_status[index-board_width]==1) ratio = .4;
		else if(x<board_width-1 && light_status[index+1]==1) ratio = .4;
		else if(y<board_height-1 && light_status[index+board_height]==1) ratio = .4;
	}

	COLOR.rgb = screen_sample.rgb * ratio;
	//COLOR.rgb = screen_sample.rgb * COLOR.rgb;
	//COLOR.a = 1.;
}