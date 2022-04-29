<?php
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

if($_SESSION['login'] == FALSE)
{
?>
	<li class="<?php echo ((defined('CURRENT_PAGE') && CURRENT_PAGE == "frontpage") ? "selected" : ""); ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/index.php">Home</a></li> 
	<li class="<?php echo ((defined('CURRENT_PAGE') && CURRENT_PAGE == "login") ? "selected" : ""); ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/login.php">Log In</a></li> 
	<li class="<?php echo ((defined('CURRENT_PAGE') && CURRENT_PAGE == "register") ? "selected" : ""); ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/register.php">Sign Up</a></li> 
<?php
}
else
{
?>
	<li class="<?php echo ((defined('CURRENT_PAGE') && CURRENT_PAGE == "home") ? "selected" : ""); ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/me.php">Home</a></li> 
	<li class="<?php echo ((defined('CURRENT_PAGE') && CURRENT_PAGE == "settings") ? "selected" : ""); ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/settings.php">Settings</a></li>
	<li class=""><a style="cursor:pointer;" onclick="if(confirm('Do you really want to logout?')){document.location='http://<?php echo SITE_DOMAIN; ?>/me.php?pa=1';}">Logout</a></li> 
<?php
}
?>