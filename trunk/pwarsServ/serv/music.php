<?php
$filename = $_GET["folder"];
if($filename && !preg_match("/^[\w\d]{1,150}$/",$filename)) exit("filename does not match regex");
foreach (glob("music/$filename/*.ogg") as $filename) {
	echo "$filename\r\n";
}
?>