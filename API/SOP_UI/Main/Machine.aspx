<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Machine.aspx.cs" Inherits="SOP_UI.Main.Machine" Async="true" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        body {
            background: radial-gradient(circle at top left, #eef6ff, #dce7f5 60%);
        }

        .machine-wrapper {
            max-width: 950px;
            margin: 30px auto;
        }

        .machine-header-card {
            background: linear-gradient(135deg, #0f172a, #0ea5e9);
            color: white;
            padding: 24px 28px;
            border-radius: 20px;
            box-shadow: 0 12px 35px rgba(14,165,233,0.25);
            display: flex;
            justify-content: space-between;
            align-items: center;
        }

        .header-title {
            font-size: 24px;
            font-weight: 700;
            letter-spacing: 0.3px;
        }

        .header-subtitle {
            font-size: 13px;
            opacity: 0.85;
            margin-top: 4px;
        }

        .download-all {
            background: white;
            color: #0ea5e9;
            padding: 9px 18px;
            border-radius: 30px;
            text-decoration: none;
            font-size: 13px;
            font-weight: 700;
            transition: 0.2s;
        }

        .download-all:hover {
            background: #f0f9ff;
            transform: translateY(-2px);
        }

        .machine-tabs {
            display: flex;
            gap: 8px;
            margin-top: 22px;
            background: #f1f5f9;
            padding: 6px;
            border-radius: 14px;
            width: fit-content;
        }

        .tab {
            padding: 10px 22px;
            border-radius: 10px;
            cursor: pointer;
            font-weight: 600;
            font-size: 14px;
            color: #475569;
            transition: 0.2s;
        }

        .tab:hover {
            background: #e2e8f0;
        }

        .tab.active {
            background: linear-gradient(135deg, #0f172a, #0ea5e9);
            color: white;
            box-shadow: 0 4px 12px rgba(14,165,233,0.3);
        }

        #searchBox {
            width: 100%;
            margin-top: 18px;
            padding: 12px 16px;
            border-radius: 12px;
            border: 1px solid #cbd5f5;
            outline: none;
            font-size: 14px;
            background: white;
            box-shadow: 0 2px 8px rgba(0,0,0,0.03);
            transition: 0.2s;
        }

        #searchBox:focus {
            border-color: #0ea5e9;
            box-shadow: 0 0 0 3px rgba(14,165,233,0.15);
        }

        /* ITEM CARD */
        .item-card {
            background: white;
            margin-top: 14px;
            border-radius: 16px;
            box-shadow: 0 4px 16px rgba(0,0,0,0.06);
            overflow: hidden;
            transition: 0.2s;
        }

        .item-card:hover {
            box-shadow: 0 8px 24px rgba(0,0,0,0.1);
        }

        .item-title {
            font-size: 16px;
            font-weight: 700;
            color: #1e293b;
            cursor: pointer;
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 16px 20px;
        }

        .item-title:hover {
            background: #f8fafc;
        }

        .item-badge {
            font-size: 11px;
            font-weight: 600;
            color: #0ea5e9;
            background: #e0f2fe;
            padding: 2px 10px;
            border-radius: 20px;
            margin-left: 10px;
        }

        .item-content {
            display: none;
            padding: 0 20px 18px;
        }

        .type-box {
            margin-top: 10px;
            padding: 14px 16px;
            background: #f8fafc;
            border-radius: 12px;
            border: 1px solid #eef2f7;
        }

        .type-box b {
            color: #0ea5e9;
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .file-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            background: white;
            padding: 10px 12px;
            border-radius: 10px;
            margin-top: 8px;
            border: 1px solid #e2e8f0;
            transition: 0.15s;
        }

        .file-row:hover {
            border-color: #0ea5e9;
            transform: translateX(2px);
        }

        .file-name {
            font-size: 13px;
            color: #334155;
            display: flex;
            align-items: center;
            gap: 8px;
        }

        .file-actions a {
            font-size: 12px;
            text-decoration: none;
            padding: 5px 12px;
            border-radius: 20px;
            margin-left: 6px;
            font-weight: 600;
        }

        .btn-preview {
            background: #e0f2fe;
            color: #0284c7;
        }

        .btn-preview:hover {
            background: #bae6fd;
        }

        .btn-download {
            background: #0ea5e9;
            color: white;
        }

        .btn-download:hover {
            background: #0284c7;
        }

        .arrow {
            transition: 0.3s;
            color: #94a3b8;
        }

        .rotate {
            transform: rotate(180deg);
        }

        /* PREVIEW MODAL */
        .modal-dialog {
            width: 90%;
            max-width: 1200px;
        }
        .modal-content {
            max-height: 700px;
            height: 50%;
            border-radius: 16px;
        }
        .modal-body {
            height: calc(90vh - 60px);
            padding: 0;
        }
        #previewFrame {
            width: 100%;
            height: 100%;
            border: none;
        }

    </style>

    <div class="machine-wrapper">

        <div class="machine-header-card">
            <div class="header-title">Machine Master File Data</div>

            <a id="downloadLink"
               runat="server"
               class="download-all"
               href="#">
               ⬇ Download
            </a>
        </div>

        <%
            var cat = Request.QueryString["categoryId"] ?? "10";
        %>

        <div class="machine-tabs">

            <div class='tab <%= cat == "10" ? "active" : "" %>' onclick="loadTab(10)">
                Assist Machine List
            </div>

            <div class='tab <%= cat == "11" ? "active" : "" %>' onclick="loadTab(11)">
                Sewing Machine List
            </div>

        </div>

        <input type="text" id="searchBox" placeholder="Search Item..." onkeyup="searchItem()" />

        <div id="machineContainer" runat="server"></div>

    </div>
    
    <div class="modal fade" id="previewModal" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Preview</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body p-0">
            <iframe id="previewFrame"></iframe>
          </div>
        </div>
      </div>
    </div>
    
    <div class="modal fade" id="noAccessModal" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered modal-sm">
        <div class="modal-content" style="border-radius:16px; text-align:center; padding:20px;">
          <div style="font-size:40px; color:#ef4444;">🚫</div>
          <h5 style="margin-top:10px;">Access Denied</h5>
          <p style="color:#64748b; font-size:14px;">
            You do not have permission to download this file.<br/>
            Contact admin to get access.
          </p>
          <button type="button" class="btn btn-main" data-bs-dismiss="modal" style="border-radius:20px;">
            OK
          </button>
        </div>
      </div>
    </div>

    <script>
        function toggleItem(el) {
            let content = el.nextElementSibling;
            let arrow = el.querySelector(".arrow");
            if (content.style.display === "block") {
                content.style.display = "none";
                arrow.classList.remove("rotate");
            } else {
                content.style.display = "block";
                arrow.classList.add("rotate");
            }
        }

        function showNoAccess() {
            var modal = new bootstrap.Modal(document.getElementById('noAccessModal'));
            modal.show();
        }

        function loadTab(categoryId) {
            window.location.href = `/Main/Machine.aspx?categoryId=${categoryId}`;
        }

        function searchItem() {
            let input = document.getElementById("searchBox").value.toLowerCase();
            document.querySelectorAll(".item-card").forEach(i => {
                i.style.display = i.innerText.toLowerCase().includes(input) ? "" : "none";
            });
        }

        function openPreview(url) {
            var frame = document.getElementById("previewFrame");
            frame.src = url + "#toolbar=0&view=FitH";
            var modal = new bootstrap.Modal(document.getElementById('previewModal'));
            modal.show();
        }

        function resizePreview() {
            var modalBody = document.querySelector("#previewModal .modal-body");
            var frame = document.getElementById("previewFrame");
            if (modalBody && frame) frame.style.height = modalBody.clientHeight + "px";
        }

        window.addEventListener("resize", resizePreview);
        document.getElementById('previewModal').addEventListener('shown.bs.modal', resizePreview);
        document.getElementById('previewModal').addEventListener('hidden.bs.modal', function () {
            document.getElementById("previewFrame").src = "";
        });
    </script>
</asp:Content>