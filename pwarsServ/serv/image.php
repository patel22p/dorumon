<?php
header('Content-type: image/jpeg');
$img = $_GET["image"];
if($img)
	echo file_get_contents($img,null,null,0,1000*100);
?>