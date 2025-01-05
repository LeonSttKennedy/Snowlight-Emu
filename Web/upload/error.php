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

// Get CMS Header Static
$GetTemplate->GetHeader(); // Website Header
$GetTemplate->GetContentHeader("Error"); // Content Header
$GetTemplate->WriteLine('<div id="main">');

// Getting the Page
$GetTemplate->GetTPL("error");
$GetTemplate->WriteLine('</div>');

// Credit Stuff
$GetTemplate->GetContentFooter();
$GetTemplate->GetFooter();
?>