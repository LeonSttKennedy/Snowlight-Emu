<?php
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

define('IN_MAINTENANCE', true);
require_once "../brain.php";
require_once "admincore.php";

$_cmd = null;

if(isset($_POST['_cmd']))
{
	$_cmd = strtolower($_POST['_cmd']);
}

if($_cmd == null && isset($_GET['_cmd']))
{
	$_cmd = strtolower($_GET['_cmd']);
}


if($_cmd == null)
{

	$initial = 'main';
	
	if(!HK_LOGGED_IN)
	{
		$initial = 'login';
		$_SESSION['HK_LOGIN_ERROR'] = "No housekeeping session found";
	}
	
	header("Location: http://" . hk_www . "/index.php?_cmd=" . $initial);
	exit;
}

if(!HK_LOGGED_IN && $_cmd != 'login')
{
	header("Location: http://" . hk_www . "/index.php?_cmd=login");
	exit;
}

switch($_cmd)
{
	case 'logout':
		session_destroy();
		session_start();
		$_SESSION['HK_LOGIN_ERROR'] = "You have been logged out successfully";
		header("Location: http://" . hk_www . "/index.php?_cmd=login");
	    exit;
	
	case 'login';
	case 'main';
	case 'home';
	
		if (HK_LOGGED_IN)
		{
			require_once 'pages/main.php';
		}
		else
		{
			require_once 'pages/login.php';
		}
		
		break;
		
	default:
	
		if (file_exists('pages/' . $_cmd . '.php') && HK_LOGGED_IN)
		{
			require_once 'pages/' . $_cmd . '.php';
		}
		else
		{
			require_once 'pages/404.php';
		}
		
		break;
}
?>