<?php   
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

// Requires all Classes and some Security Settings
require_once 'brain.php';

// Website Header
$GetTemplate->GetHeader();

$GetTemplate->Init();

if($_GET['id'] == "")
{
	$query = mysql_query("SELECT * FROM articles ORDER BY id DESC LIMIT 1;");
	$news = mysql_fetch_assoc($query);
	$GetSecurity->Redirect("./news.php?id=".$news['id']);
}

// Content Header
$GetTemplate->GetContentHeader("News");

$GetTemplate->WriteLine('<div id="main">');

// Getting the Page
$GetTemplate->GetTPL("news");

$GetTemplate->WriteLine('</div>');

// Getting Credits Stuff
$GetTemplate->GetContentFooter();

$GetTemplate->GetFooter();
?>