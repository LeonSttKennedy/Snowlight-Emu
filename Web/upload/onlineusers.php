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
// Lets Check is Sessions is Registred
$GetSecurity->Get_Session();

echo '<b>The following users are currently online:</b>';

$getUsers = mysql_query("SELECT * FROM characters WHERE online = '1' ORDER BY activity_points_last_update DESC");

if (mysql_num_rows($getUsers) > 0)
{
	echo '<ul style="margin: 0;">';
	
	while ($u = mysql_fetch_assoc($getUsers))
	{
		echo '<li style="margin-left: 20px;">';
		echo $u['username'];
		echo '</li>';
	}
	
	echo '</ul>';
}
else
{
	echo '<br /><br /><i>There are currently no users online.</i>';
}

?>