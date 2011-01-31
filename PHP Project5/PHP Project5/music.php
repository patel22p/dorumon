<?php
foreach (glob("music/$_GET[folder]/*.ogg") as $filename) {
	echo "$filename\r\n";
}
?>