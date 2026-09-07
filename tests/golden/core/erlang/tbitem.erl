-module(tbitem).
-export([get_data_map/0, get_key_list/0]).

get_data_map() -> #{
	1 => #{'id' => 1,'name' => "sword",'quality' => 1,'tags' => ["weapon","melee"],'attrs' => #{"atk" => 10,"def" => 2},'flags' => [1,2],'shape' => #{'name__' => "Circle",'id' => 10,'radius' => 1.5}},
	2 => #{'id' => 2,'name' => "shield",'quality' => 2,'tags' => ["armor"],'attrs' => #{"def" => 20},'flags' => [3],'shape' => #{'name__' => "Rect",'id' => 11,'width' => 2.0,'height' => 3.0}}
}.

get_key_list() ->
	[1, 2].
