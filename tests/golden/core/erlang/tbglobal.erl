-module(tbglobal).
-export([get_data/0]).

get_data() ->
	#{'version' => 3,'title' => "demo"}.
