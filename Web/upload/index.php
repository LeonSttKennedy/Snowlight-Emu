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

define("CURRENT_PAGE", "frontpage");
// Requires
$GetSecurity->IF_LogIN();

$GetTemplate->Init();

// Get CMS Header Static
$GetTemplate->GetHeader(); // Website Header
$GetTemplate->GetContentHeader("Home"); // Content Header
$GetTemplate->WriteLine('<div id="main">');

// Get Text - Came from frontpage
$GetTemplate->WriteLine('<div class="column1" id="column1">');
$GetTemplate->GetTEXT("frontpage");
$GetTemplate->WriteLine('</div>');
$GetTemplate->WriteLine('<div class="column2" id="column2">');
$GetTemplate->WriteLine('<div class="module texthtml">');
$GetTemplate->WriteLine('<div class="content texthtml">');



// Get News Stuff
$GetTemplate->WriteLine('<div class="module newslist">');
$GetTemplate->WriteLine('<div id="newsblock">');
$GetTemplate->GetTPL("news_articles");
$GetTemplate->WriteLine('<div class="inner">');
// LAST NEW
$GetTemplate->WriteLine('</div>');

// The rest of HTML
$GetTemplate->WriteLineSeveral('</div>', 3);
$GetTemplate->WriteLine('<div class="module texthtml">');
$GetTemplate->WriteLine('<div class="content texthtml">');
$GetTemplate->WriteLine('<div style="text-align: center;">');
$GetTemplate->WriteLineSeveral('</div>', 3);
$GetTemplate->WriteLine('<div class="module texthtml">');
$GetTemplate->WriteLine('<div class="content texthtml">');
$GetTemplate->WriteLineSeveral('</div>', 3);
$GetTemplate->WriteLine('<div class="clear">');

// Get the contents of Footer
$GetTemplate->GetContentFooter();
$GetTemplate->GetFooter();
?>
