<?php
header('Content-type: image/jpeg');
echo file_get_contents($_GET["image"],null,null,0,1000*100);
?>