<?php
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

define('IN_HK', true);
define('hk_www', SITE_DOMAIN . '/manage');

define('HK_LOGGED_IN', $_SESSION['hk_login']);
define('LOGGED_IN', $_SESSION['login']);
define('USER_NAME', $_SESSION['account_name']);

function fMessage($type, $message)
{
	if (isset($_SESSION['fmsg']))
	{
		return;
	}
	
	$_SESSION['fmsg_type'] = $type;
	$_SESSION['fmsg'] = $message;
}

function returnBadgeCode($bid)
{
	$bcode = mysql_result(mysql_query("SELECT code FROM badge_definitions WHERE id = '$bid' LIMIT 1"), 0);
	return $bcode;
}
?>