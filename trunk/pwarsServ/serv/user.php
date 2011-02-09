<?php
include 'common.php';
$path ="users/$nick.txt";		



if(!$nick) exit("wrong params");
if($login) 
{
	Password($nick,$passw);
	exit(Success);
}
if($registr)
{
	if(!$nick || !$passw || !$xml) exit("wrong params");		
	
	if(PasswordCheck($nick,$passw)) exit(Success);
	
	$filetext =  file_get_contents("users.txt");
	if($filetext)
	{
		$stringArray = explode("\r\n",$filetext);			
		for ($i = 0; $i < count($stringArray); $i++) {
			$row = explode("\t",$stringArray[$i]);
			if($row[0] == $nick)
				exit("User Already Exists");			
		}
	}		
	 
//	if(file_exists("ips.txt"))
//	{  
//		$d = date("d", filemtime("ips.txt"));
//		$d2 = date("d", time());
//		if($d != $d2)
//			unlink("ips.txt");
//	}
//	if(file_exists("ips.txt"))
//	{
//		$ips = explode("\r\n",file_get_contents("ips.txt"));
//		$count = array_count_values($ips);		
//		if($count[$_SERVER["REMOTE_ADDR"]]>3)
//			exit("registration limit reached, try tommorow ".(60*60*24-$timeLeft));	
//	}	
//	file_put_contents("ips.txt","$_SERVER[REMOTE_ADDR]\r\n",FILE_APPEND);	
	file_put_contents("users.txt","$nick\t$passw\r\n",FILE_APPEND) or exit("cannot write file");		
}

if($xml)
{
	Password($nick,$passw);
	file_put_contents($path,$xml) or exit("Cannot save xml");		
	exit(Success);							
}
if(!file_exists($path)) exit ("User xml not exists");
echo file_get_contents($path);


?>