<h3>Account settings</h3>
<p>Update your preferencies!</p>
<br />
<form method="post" action="settings.php?pa=1">


<table width="100%" class="registration">

<?php
$query = mysql_query("SELECT * FROM characters WHERE username = '".$_SESSION['account_name']."'");
$get = mysql_fetch_assoc($query);
$freqs = $get['privacy_accept_friends'];
$mimic = $get['allow_mimic'];
$gifts = $get['allow_gifts'];
?>

<tr><td class="l" width="30%">
<strong>Friendship request</strong><p><small>Want to receive friend requests?</small></p></td>
<td class="r" width="100%"><input type="checkbox"  name="friendreqs" <?php if($freqs == "1") { echo "checked=\"on\""; } elseif($freqs == "0") { echo ""; } ?> />&nbsp;&nbsp;<?php if($freqs == "0") { echo "You're not receiving friend requests"; } else if($freqs == "1") { echo "You're receiving friend requests"; } ?><br /></td></tr>

<tr><td class="l" width="30%">
<strong>Allow Mimic</strong><p><small>Don't wanna other user copying your fantastic look?</small></p></td>
<td class="r" width="100%"><input type="checkbox"  name="mimic" <?php if($mimic == "1") { echo "checked=\"on\""; } elseif($mimic == "0") { echo ""; } ?> />&nbsp;&nbsp;<?php if($mimic == "0") { echo "You're no longer able to be mimicked"; } else if($mimic == "1") { echo "You're now able to be mimiced"; } ?><br /></td></tr>

<tr><td class="l" width="30%">
<strong>Gifts</strong><p><small>Don't wanna receive gifts?</small></p></td>
<td class="r" width="100%"><input type="checkbox"  name="gifts" <?php if($gifts == "1") { echo "checked=\"on\""; } elseif($gifts == "0") { echo ""; } ?> />&nbsp;&nbsp;<?php if($gifts == "0") { echo "You're not receiving gifts"; } else if($gifts == "1") { echo "You're receiving gifts"; } ?><br /></td></tr>

<tr>
<td colspan="2" class="r" style="text-align: center;">
<br />
<br />
<tr><td colspan="2" class="r"><input type="submit" class="blbtn"; value="Update" /></td></tr></table>

</form>
