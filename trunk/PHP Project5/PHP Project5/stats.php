<?php
include 'common.php';


$path = "games/stats-$game.txt";

if(!$game) 
	exit("wrong params");

//check for password
if($passw)
	Password($nick,$passw);		

//read tabble
if(file_exists($path))	
{
	$filetext =  file_get_contents($path);
	$stringArray = explode("\r\n",$filetext);	
	for ($i = 0; $i < count($stringArray); $i++) {	
		$values = explode("\t",$stringArray[$i]);	
		if(count($values)==3)
			$tabble["$values[0]"] = "$values[1]\t$values[2]";
	}
}
//write user, sort tabble
$write = $passw;		

if($write && (!isset($frags) || !isset($deaths)) ) exit("wrong params: no frags");
if($write)
{	
	if($tabble[$nick]) //max score check
	{
		$plScores = explode("\t", $tabble[$nick]);	
		$tabble[$nick] = max($plScores[0],$frags)."\t".max($plScores[1],$deaths);		
	}
	else
		$tabble[$nick] = "$frags\t$deaths";		
	arsort($tabble,SORT_NUMERIC);
	$handle = fopen($path, 'w+') 
		or exit("cannot open file");
}
//write table to file and print
$Index = 0;$UserId =0;
if($tabble)
{
	foreach ($tabble as $key => $val) {    
		if($write)
			fwrite($handle,"$key\t$val\r\n");	
		if($nick)
			if($key == $nick) $UserId = $Index;
		$newStringArray[$Index] = "$key\t$val\r\n";				
		$Index++;
	}
	if($write)
		fclose($handle);
	print("table:$game\r\n");
	//print tabble
	
	for ($i = 0; $i < count($newStringArray); $i++) {	
		if(($UserId-5 < $i && $i < $UserId+5 && !$find) || ($find &&  strstr($newStringArray[$i],$find)))
			print(($i+1)."\t$newStringArray[$i]");	
	}
}
else echo "table not found $game";

?>