<?php

header("Content-Type: text/plain");

if(!file_exists("users")) mkdir("users");
if(!file_exists("games")) mkdir("games");
if(!file_exists("users.txt")) file_put_contents("users.txt","",FILE_APPEND);

error_reporting(0);
set_error_handler("myErrorHandler");
define("Success", "Success");
date_default_timezone_set('UTC');
$dbg = $_GET["DBGSESSID"] | $_GET["XDEBUG_SESSION_START"];
$nick = $dbg?"sadadsdtdsd":$_GET["user"];
$registr = $dbg?"1":$_GET["reg"];
$passw = $dbg?"0CC175B9C0F1B6A831C399E269772661":$_GET["passw"];		
$email = $dbg?"":$_GET["email"];
$find = $dbg?"":$_GET["find"];
$game = $dbg?"":$_GET["game"];
$frags = $dbg?"":$_GET["frags"]; 
$deaths = $dbg?"":$_GET["deaths"];
$xml = stripcslashes($dbg?"asd":$_POST["xml"]);	
$login = $dbg?"":$_GET["login"];	


if(!$dbg)
{
	$hash = $_POST["hash"];
	$req = trim(substr(strrchr($_SERVER["REQUEST_URI"], "/"), 1));
	$myhash= md5($req."er54s4");
	if(strtolower($hash)!= strtolower($myhash))
		exit("wrong hash $req");
}	
if($nick && !preg_match("/^[\w_\d]{1,15}$/",$nick)) exit("nick must not have special characters, length from 4 to 10");
if($passw && !preg_match("/^[\w\d]{1,150}$/",$passw)) exit("password does not match regex");

function myErrorHandler($errno, $errstr, $errfile, $errline) {
	if($errno != 8) 
		echo "$errno\t$errfile\tLine:$errline\t$errstr\r\n";
}
function Password($nick,$passw)
{
	if(PasswordCheck($nick,$passw)) return true;
	else
		exit("pasword doest not match");
}
function PasswordCheck($nick,$passw) 
{
	if(!$passw || !$nick) return false;
	$userstext =  file_get_contents("users.txt"); 
	if(!$userstext) return false;
	$ussersArray = explode("\r\n",$userstext);	
	if($ussersArray  && in_array("$nick\t$passw",$ussersArray))
		return true;
	return false;
}
?>