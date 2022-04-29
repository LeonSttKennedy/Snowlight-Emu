<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hotel_admin'))
{
	exit;
}

if(isset($_GET["update"]))
{
	fMessage('error', 'Woops, under construction.');
	header("Location: index.php?_cmd=serversettings");
	exit;
}

function GetServerSettings($setting)
{
	return mysql_result(mysql_query("SELECT $setting FROM server_settings LIMIT 1"), 0);
}

function GetServerTexts($text)
{
	return mysql_result(mysql_query("SELECT display_text FROM server_ingame_texts WHERE identifier = '$text' LIMIT 1"), 0);
}

require_once "top.php";

$motdtext = explode("|", GetServerSettings(motd_text));

?>
<h1>Server settings</h1>

<br />

<p>
	This tool allows you to edit predefined server settings.
</p>
<h2>Server settings to edit</h2>
<br />
<form method="post" action="index.php?_cmd=serversettings&update">
	<table width="100%">
		<tr>
			<td style="border-width: 2px; border-style:dashed; border-color:#93c1a7;">
				<h2 style="margin: 0;">Activity Points</h2><br />
				<p style="margin: 5px 0 0 5px;">
					<input type="checkbox" name="activitypointsenabled" <?php echo (GetServerSettings(activitypoints_enabled) == "1" ? 'checked="on"' : ''); ?> />&nbsp;&nbsp;Users <?php echo (GetServerSettings(activitypoints_enabled) == "1" ? '' : 'do not'); ?> will win bonuses while logged in.
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Interval (in Min):<br /><input type="number" name="activitypointsinterval" style="text-align:center;" value="<?php echo GetServerSettings(activitypoints_interval) / 60; ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Credits:<br /><input type="number" name="activitypointscredits" style="text-align:center;" value="<?php echo GetServerSettings(activitypoints_credits_amount); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Pixels:<br /><input type="number" name="activitypointspixels" style="text-align:center;" value="<?php echo GetServerSettings(activitypoints_pixels_amount); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					<input type="checkbox" name="activitypointstovipenabled" <?php echo (GetServerSettings(more_activitypoints_for_vip_users) == "1" ? 'checked="on"' : ''); ?> />&nbsp;&nbsp;Users with VIP Subscription <?php echo (GetServerSettings(more_activitypoints_for_vip_users) == "1" ? '' : 'do not'); ?> will win more bonuses while logged in.
				</p>
				<p style="margin: 5px 0 5px 5px;">
					To VIP Credits:<br /><input type="number" name="activitypointstovipcredits" style="text-align:center;" value="<?php echo GetServerSettings(more_activitypoints_credits_amount); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					To VIP Pixels:<br /><input type="number" name="activitypointstovippixels" style="text-align:center;" value="<?php echo GetServerSettings(more_activitypoints_pixels_amount); ?>" />
				</p>
			</td>
		</tr>
		<tr>
			<td style="border-width: 2px; border-style:dashed; border-color:#93c1a7;">
				<h2 style="margin: 0;">Message of the day</h2><br />
				<div style="float: left; width: 57%;">
					<p style="margin: 5px 0 0 5px;">
						<input type="checkbox" name="motdenabled" <?php echo (GetServerSettings(motd_enabled) == "1" ? 'checked="on"' : ''); ?> />&nbsp;&nbsp;Is now <?php echo (GetServerSettings(motd_enabled) == "1" ? 'Enabled' : 'Disabled'); ?>
					</p>
					<p style="margin: 5px 0 5px 5px;">
						Type:<br />
						<div class="hideradio">
							<input type="radio" id="MessageOfTheDayComposer" name="motdtype" value="MessageOfTheDayComposer" <?php echo (GetServerSettings(motd_type) == "MessageOfTheDayComposer" ? 'checked' : ''); ?>>
							<label for="MessageOfTheDayComposer">
								<img src="images/alerts/MessageOfTheDayComposer.png" title="MessageOfTheDayComposer" width="200" height="100" />
								MessageOfTheDayComposer
							</label>
							<input type="radio" id="NotificationMessageComposer" name="motdtype" value="NotificationMessageComposer" <?php echo (GetServerSettings(motd_type) == "NotificationMessageComposer" ? 'checked' : ''); ?>>
							<label for="NotificationMessageComposer">
								<img src="images/alerts/NotificationMessageComposer.png" title="NotificationMessageComposer" width="200" height="100" />
								NotificationMessageComposer
							</label>
						</div>
					</p>
					<p style="margin: 5px 5px 5px 5px;">
						Text:<br />
						<textarea name="motdtext" cols="50" rows="4" /><?php echo $motdtext[0]; ?></textarea>
					</p>
					<p style="margin: 5px 0 5px 5px;">
						Link:<br /><input type="text" name="motdlink" value="<?php echo $motdtext[1]; ?>" />
					</p>
				</div>
				<div style="float: right; width: 41%;">
					<div style="border-style: solid; border-width: 0 0 0 2px; border-color:darkred; padding: 0 0 0 5px; margin: 5px 0 0 0;">
						<p>
							<b style="color: darkred; font-size: 25px; padding: 0 0 0 0; margin: 0 8px 0 0;">&RightArrow;</b>You can use "<u>\n</u>" to enter a new line in MOTD.
						</p>
					</div>
					<div style="border-style: solid; border-width: 0 0 0 2px; border-color:darkred; padding: 0 0 0 5px; margin: 5px 0 0 0;">
						<p>
							<b style="color: darkred; font-size: 25px; padding: 0 0 0 0; margin: 0 8px 0 0;">&RightArrow;</b>To enter a MOTD with link you must to select "<u>NotificationMessageComposer</u>" in type.
						</p>
					</div>
				</div>
				<div style="clear: both;">
				</div>
			</td>
		</tr>
		<tr>
			<td style="border-width: 2px; border-style:dashed; border-color:#93c1a7;">
				<h2 style="margin: 0;">Wordfilter settings</h2><br />
				<p style="margin: 5px 0 0 5px;">
					Occurrences until be mute:<br /><input type="number" name="wordfiltertotalocorrencies" style="text-align:center;" value="<?php echo GetServerSettings(wordfilter_maximum_count); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Time Muted (in Min):<br /><input type="number" name="wordfiltertimemuted" style="text-align:center;" value="<?php echo GetServerSettings(wordfilter_time_muted) / 60; ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Wordfilter Replacement Word:<br /><input type="text" name="wordfilterreplacementword" value="<?php echo GetServerTexts(wordfilter_replacement_word); ?>" />
				</p>
			</td>
		</tr>
		<tr>
			<td style="border-width: 2px; border-style:dashed; border-color:#93c1a7;">
				<h2 style="margin: 0;">Marketplace settings</h2><br />
				<p style="margin: 5px 0 0 5px;">
					<input type="checkbox" name="marketplaceenabled" <?php echo (GetServerSettings(marketplace_enabled) == "1" ? 'checked="on"' : ''); ?> />&nbsp;&nbsp;Users <?php echo (GetServerSettings(marketplace_enabled) == "1" ? '' : 'do not'); ?> can sell items in marketplace.
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Tax (in %):<br /><input type="range" id="marketplace_tax" name="marketplacetax" min="1" max="100" value="<?php echo GetServerSettings(marketplace_tax); ?>" />&nbsp;&nbsp;Current Tax: <span id="currentmarketplace_tax"></span>%
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Marketplace Tokens Price:<br /><input type="number" name="marketplacetokensprice" style="text-align:center;" value="<?php echo GetServerSettings(marketplace_tokens_price); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Marketplace Tokens for normal users:<br /><input type="number" name="marketplacetokens" style="text-align:center;" value="<?php echo GetServerSettings(marketplace_default_tokens); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Marketplace Tokens for HC/VIP users:<br /><input type="number" name="marketplacepremiumtokens" style="text-align:center;" value="<?php echo GetServerSettings(marketplace_premium_tokens); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Marketplace Offer min price:<br /><input type="number" name="marketplaceofferminprice" style="text-align:center;" value="<?php echo GetServerSettings(marketplace_min_price); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Marketplace Offer max price:<br /><input type="number" name="marketplaceoffermaxprice" style="text-align:center;" value="<?php echo GetServerSettings(marketplace_max_price); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Marketplace Offer total hours:<br /><input type="number" name="marketplaceofferhours" style="text-align:center;" value="<?php echo GetServerSettings(marketplace_offer_hours); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Marketplace avarage days:<br /><input type="number" name="marketplaceavaragedays" style="text-align:center;" value="<?php echo GetServerSettings(marketplace_avarage_days); ?>" />
				</p>
			</td>
		</tr>
		<tr>
			<td style="border-width: 2px; border-style:dashed; border-color:#93c1a7;">
				<h2 style="margin: 0;">Login badge</h2><br />
				<div style="float: left; width: 57%;">
					<p style="margin: 5px 0 0 5px;">
						<input type="checkbox" name="badgewhenloginenabled" <?php echo (GetServerSettings(login_badge_enabled) == "1" ? 'checked="on"' : ''); ?> />&nbsp;&nbsp;Win a badge when login, is now <?php echo (GetServerSettings(login_badge_enabled) == "1" ? 'Enabled' : 'Disabled'); ?>.
					</p>
					<br />
					<p style="margin: 0 0 5px 5px;">
						Enter badge definition ID:<br /><input type="text" name="badgewhenloginid" style="text-align:center;" value="<?php echo GetServerSettings(login_badge_id); ?>" />
					</p>
				</div>
				<div style="float: right; width: 41%;">
					<p>
						Current Badge:
					</p>
					<img src="<?php echo CLIENT_BASE . '/c_images/album1584/' . returnBadgeCode(GetServerSettings(login_badge_id)) . '.gif'; ?>" alt="Current Badge" />
				</div>
				<div style="clear: both;"></div>
				<p style="margin: 5px;">
					To see all badge definition ids <a href="index.php?_cmd=badgedefs">click here</a>.
				</p>
			</td>
		</tr>
		<tr>
			<td style="border-width: 2px; border-style:dashed; border-color:#93c1a7;">
				<h2 style="margin: 0;">Other settings</h2><br />	
				<p style="margin: 5px 0 0 5px;">
					<input type="checkbox" name="badgewhenloginenabled" <?php echo (GetServerSettings(gifting_system_enabled) == "1" ? 'checked="on"' : ''); ?> />&nbsp;&nbsp;Users <?php echo (GetServerSettings(login_badge_enabled) == "1" ? '' : 'do not'); ?> can send gifts.
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Max favorites per user:<br /><input type="number" name="maxfavoritesperuser" style="text-align:center;" value="<?php echo GetServerSettings(max_favorites_per_user); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Max furni per room:<br /><input type="number" name="maxfurniperroom" style="text-align:center;" value="<?php echo GetServerSettings(max_furni_per_room); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Max furni stacking:<br /><input type="number" name="maxfurnistacking" style="text-align:center;" value="<?php echo GetServerSettings(max_furni_stacking); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Max pets per room:<br /><input type="number" name="maxpetsperroom" style="text-align:center;" value="<?php echo GetServerSettings(max_pets_per_room); ?>" />
				</p>
				<p style="margin: 5px 0 5px 5px;">
					Max room per user:<br /><input type="number" name="maxroomperuser" style="text-align:center;" value="<?php echo GetServerSettings(max_rooms_per_user); ?>" />
				</p>
			</td>
		</tr>
		<tr>
			<td>
				<input type="submit" value="Update">&nbsp;&nbsp;<input type="button" value="Cancel/Reset" onclick="location.reload(true);">
			</td>
		</tr>
	</table>
</form>
<script>
var slider = document.getElementById('marketplace_tax');
var output = document.getElementById('currentmarketplace_tax');
output.innerHTML = slider.value;

slider.oninput = function() {
	output.innerHTML = this.value;
}
</script>
<?php
require_once "bottom.php";
?>