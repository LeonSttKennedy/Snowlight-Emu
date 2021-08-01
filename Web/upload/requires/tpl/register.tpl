<h3>Account Registration</h3>
<p>Signing up is free, easy, and it only takes a minute. All we need is some (very) basic information.</p>
<br />

<form method="post" action="http://<?php echo SITE_DOMAIN; ?>/register.php?pa=1">

<table width="100%" class="registration">


<tr><td class="l" width="55%">
<strong>E-mail address:</strong><p><small>We will contact you using this e-mail address. You will use your e-mail address to log in.</small></p></td>
<td class="r" width="45%"><input class="txt fs120" type="text" name="email" maxlength="255" value=""></td></tr>


<tr><td class="l"><strong>Username:</strong><p><small>Your character name.</small></p></td>
<td class="r"><input class="txt fs120" type="text" name="username" maxlength="16" value=""></td></tr>


<tr><td class="l"><strong>Password:</strong><p><small>You will need this password to sign in to your account.</small></p></td>
<td class="r"><input class="txt fs120" type="password" name="password" maxlength="255" value=""></td></tr>

<tr><td class="l"><strong>Confirm password:</strong><p><small>Please confirm your selected password to ensure that you have typed it correctly.</small></p></td><td class="r">
<input class="txt fs120" type="password" name="password2" maxlength="255" value="">
</td></tr>

<tr>
<td colspan="2" class="r" style="text-align: center;">
<br />
<br />
<input type="checkbox" name="rules" value="i-accept">
 Yes, I have read and accept <a href="http://<?php echo SITE_DOMAIN; ?>/rules.php" target="_blank">the rules</a>.
<br />
<br />
<tr><td colspan="2" class="r" style="text-align: center;"><input type="submit" class="btn" value="Complete registration">&nbsp;<input type="button" class="btn" value="Cancel" onclick="if(confirm('Are you sure you want to cancel the registration? You will lose all your data.')){document.location='index.php';}"></td></tr></table></form>