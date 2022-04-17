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
	<li class="<?php if($current_subpage == "frontpage") { echo "selected"; } else {} ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/index.php">Home</a></li> 
	<li class="<?php if($current_subpage == "login") { echo "selected"; } else {} ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/login.php">Log In</a></li> 
	<li class="<?php if($current_subpage == "register") { echo "selected"; } else {} ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/register.php">Sign Up</a></li> 
<?php
}
else
{
?>
	<li class="<?php if($current_subpage == "home") { echo "selected"; } else {} ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/me.php">Home</a></li> 
	<li class="<?php if($current_subpage == "account_settings") { echo "selected"; } else {} ?>"><a href="http://<?php echo SITE_DOMAIN; ?>/settings.php">Settings</a></li> 

	<?php
	$uid = Users::Name2Id($_SESSION['account_name']);
	if(Users::HasRight($uid, "hk_login"))
	{
	?>
		<li class=""><a href="http://<?php echo SITE_DOMAIN; ?>/manage/">Housekeeping</a></li> 
	<?php
	}
	?>
	<li class=""><a style="cursor:pointer;" onclick="if(confirm('Do you really want to logout?')){document.location='http://<?php echo SITE_DOMAIN; ?>/me.php?pa=1';}">Logout</a></li> 
<?php
}
?>