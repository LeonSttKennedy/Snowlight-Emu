	<div class="serverstatus">
		<div class="onlinecounter">              
			<img src="http://<?php echo SITE_DOMAIN; ?>/images/icon_users.png" /> <?php CheckUsersOnline(); ?>&nbsp;
		</div> 
<?php
if($_SESSION['login'])
{
	$enabled = "<button class=\"lightgreen\" onclick=\"window.open('client.php', 'client', 'width=1100,height=700,location=false,status=false,menubar=false,directories=false,toolbar=false,resizable=true,scrollbars=false'); return false;\">Enter " . SITE_NAME . "</button>";
	$disabled = "<button class=\"disabled\" onclick=\"window.alert('" . SITE_NAME . " is currently closed. Sorry for this inconvenience!'); return false;\">" . SITE_NAME . " is currently offline</button>";
	
	echo Core::GetSystemStatus() == "1"? $enabled : $disabled; 
	
	if(Users::HasRight($_SESSION['id'], "hk_login"))
	{
?>
		<button class="red" onclick="window.location = 'http://<?php echo SITE_DOMAIN; ?>/manage/';">Housekeeping</button>
<?php
	}
}
?>
	</div>
	<div style="clear: both;"></div> 
</div> 