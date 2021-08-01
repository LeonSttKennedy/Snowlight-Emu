<?php
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

define(IN_MAINTENANCE, TRUE);

require_once "brain.php";

// If website isn´t anymore in Maintenance Mode , redirects to the frontpage (index.php)
if(!defined('MAINT_STATUS') || !MAINT_STATUS) 
{
	echo "<script type='text/javascript'>window.top.location='http://" . SITE_DOMAIN . "/';</script>";
	//header("Location: http://" . SITE_DOMAIN . "/"); 
	exit;
}
else if($GetTemplate->isLogged() && $GetUsers->HasRight($_SESSION['account_name'], "fuse_ignore_maintenance"))
{
	echo "<script type='text/javascript'>window.top.location='http://" . SITE_DOMAIN . "/';</script>";
	//header("Location: http://" . SITE_DOMAIN . "/"); 
	exit;
}
?>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title><?php echo SITE_NAME; ?> - Maintenance</title>
	<link rel="stylesheet" href="http://<?php echo SITE_DOMAIN; ?>/css/maintenance.css" />
	<link rel="shortcut icon" href="icon.ico" type="image/x-icon" /> 
	<script src="http://code.jquery.com/jquery-latest.js"></script>
</head>
<body> 

<div id="wrapper">
	
	<div id="toptop"></div>
		<div id="top"></div>
		<div id="says">
		<marquee>
			<strong>Last news:</strong> <?php echo $GetHotel->GetDatabaseConfig("maintenance_text"); ?>
		</marquee>
		</div>
	
		<div id="progress"></div>
			<div id="header"></div>
			<div class="sides">
				<img src="images/maintenance/crazzy_day.png" title="Fire Man" align="right" />
				<h2>Oops! We are in maintenance !</h2>
				<h3><?php echo SITE_NAME?> is under maintenance, we are fixing bugs for your total <i>Fun</i>.</h3><br />
				Check back later to see if we are online again!<br /><br />
				<p>Sorry for the inconvenience, but this is necessary.</p>
				<br /><br /><br /><br />
				<?php $GetTemplate->GetTPL("maintenance_footer"); ?>
			</div>
		<div id="footer">
		</div>
</div>

</body> 
</html>