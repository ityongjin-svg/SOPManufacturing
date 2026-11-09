<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SOP_UI.Login" Async="true" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Yongjin Javasuka | SOP System</title>
    <link rel="icon" type="image/x-icon" href="assets/img/logoYongjin.png"/>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0-alpha.6/css/bootstrap.min.css"/>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css"/>


<style>
/* =====================
   COLOR THEME VARIABLES
   ===================== */
    :root {
        --bg-main: #0b1220;
        --card-bg: #111827;
        --text-main: #e5e7eb;
        --text-muted: #9ca3af;
        --accent: #3b82f6;
        --border: #1f2937;
    }

    /* DARK MODE */
    body.dark {
        --bg-main: #f1f5f9;
        --card-bg: #ffffff;
        --text-main: #0f172a;
        --text-muted: #64748b;
        --accent: #2563eb;
        --border: #cbd5e1;
    }

    body {
        min-height: 100vh;
        background: linear-gradient(135deg, var(--bg-main), #dbeafe);
        font-family: 'Segoe UI', sans-serif;
        color: var(--text-main);
        transition: background .3s ease;
    }

    /* subtle texture */
    body::before {
        content: "";
        position: fixed;
        inset: 0;
        background-image: radial-gradient(rgba(0,0,0,.04) 1px, transparent 1px);
        background-size: 22px 22px;
        pointer-events: none;
    }

    .login-wrapper {
        min-height: 100vh;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .login-box {
        width: 420px;
        background: var(--card-bg);
        border-radius: 18px;
        padding: 36px;
        box-shadow: 0 25px 50px rgba(0,0,0,.25);
        transition: background .3s ease;
    }

    /* Brand */
    .brand {
        text-align: center;
        margin-bottom: 25px;
    }
    .brand img {
        width: 60px;
    }
    .brand h1 {
        font-size: 18px;
        font-weight: 700;
        margin-top: 10px;
    }
    .brand span {
        font-size: 13px;
        color: var(--text-muted);
    }

    /* Role badge */
    .role-badge {
        text-align: center;
        font-size: 12px;
        padding: 4px 12px;
        border-radius: 20px;
        background: rgba(37,99,235,.1);
        color: var(--accent);
        margin-bottom: 20px;
        display: inline-block;
    }

    /* Inputs */
    label {
        font-size: 13px;
        font-weight: 600;
    }
    .form-control {
        height: 45px;
        border-radius: 10px;
        border: 1px solid var(--border);
        background: transparent;
        color: var(--text-main);
    }
    .form-control:focus {
        border-color: var(--accent);
        box-shadow: 0 0 0 3px rgba(37,99,235,.25);
    }

    /* Password toggle */
    .password-wrapper {
        position: relative;
    }
    .toggle-password {
        position: absolute;
        right: 12px;
        top: 55px;
        transform: translateY(-50%);
        cursor: pointer;
        font-size: 13px;
        color: var(--text-muted);
    }

    /* Button */
    .btn-login {
        width: 100%;
        height: 46px;
        border-radius: 24px;
        border: none;
        background: linear-gradient(135deg, #1e293b, var(--accent));
        color: #fff;
        font-weight: 600;
        transition: all .3s ease;
    }
    .btn-login:hover {
        transform: translateY(-2px);
        box-shadow: 0 10px 20px rgba(0,0,0,.3);
    }

    /* Status */
    #lblStatus {
        display: block;
        margin-top: 12px;
        text-align: center;
        font-weight: 600;
    }

    /* Dark mode toggle */
    .toggle-dark {
        position: absolute;
        top: 20px;
        right: 20px;
        cursor: pointer;
        font-size: 14px;
        color: #414141;
    }

    .toggle-dark i {
        margin-right: 6px;
    }
    .toggle-password i {
        font-size: 16px;
    }

    /* Dark mode toggle color adjustment */
    body.dark .toggle-dark {
        color: #b37800;
    }

    body.dark .toggle-dark i.fa-sun {
        text-shadow: 0 0 6px rgb(250 204 21);
    }
</style>

<script>
    function togglePassword() {
        const pwd = document.getElementById('<%= txtPass.ClientID %>');
        const eye = document.getElementById("eyeIcon");

        if (pwd.type === "password") {
            pwd.type = "text";
            eye.className = "fa-regular fa-eye-slash";
        } else {
            pwd.type = "password";
            eye.className = "fa-regular fa-eye";
        }
    }

    function toggleDark() {
        document.body.classList.toggle("dark");

        const icon = document.getElementById("darkIcon");
        const text = document.getElementById("darkText");

        if (document.body.classList.contains("dark")) {
            icon.className = "fa-solid fa-sun";
            text.innerText = "Light Mode";
        } else {
            icon.className = "fa-solid fa-moon";
            text.innerText = "Dark Mode";
        }
    }
</script>
</head>

<body>
<form runat="server">

<div class="toggle-dark" onclick="toggleDark()">
    <i id="darkIcon" class="fa-solid fa-moon"></i>
    <span id="darkText">Dark Mode</span>
</div>


<div class="login-wrapper">
    <div class="login-box role-operator">

        <div class="brand">
            <img src="Main/img/logoYongjin.png"/>
            <h1>Yongjin Javasuka Garment</h1>
            <span>SOP Manufacturing</span>
        </div>

        <div class="text-center">
            <span class="role-badge">
                <i class="fa-solid fa-user-gear"></i>
            </span>
        </div>

        <div class="form-group">
            <label>Username</label>
            <asp:TextBox ID="txtUser" runat="server" CssClass="form-control" Placeholder="Username" 
                onkeyup="detectRole()"></asp:TextBox>
        </div>

        <div class="form-group password-wrapper">
            <label>Password</label>
            <asp:TextBox ID="txtPass" runat="server" TextMode="Password" Placeholder="Password"
                CssClass="form-control"></asp:TextBox>
            <span class="toggle-password" onclick="togglePassword()">
                <i id="eyeIcon" class="fa-regular fa-eye"></i>
            </span>
        </div>

        <asp:Button ID="btnLogin" runat="server" Text="LOGIN"
            CssClass="btn btn-login" OnClick="btnLogin_Click"/>

        <asp:Label ID="lblMsg" runat="server" ForeColor="Red" />

    </div>
</div>

</form>
</body>
</html>