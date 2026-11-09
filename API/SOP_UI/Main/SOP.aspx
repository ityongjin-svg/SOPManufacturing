<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SOP.aspx.cs" Inherits="SOP_UI.Main.SOP" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        body {
            /*background: linear-gradient(135deg, #fff7ed, #ffe4d6);*/
            background: radial-gradient(circle at top left, #e6f8ff, #dae6f2 60%);
        }

        /* CONTAINER */
        .sop-wrapper {
            max-width: 1200px;
            margin: auto;
        }

        /* SECTION CARD */
        .section-card {
            background: #f8fafc;
            border-radius: 20px;
            padding: 25px;
            box-shadow: inset 0 0 40px rgba(0,0,0,0.05);
            margin-bottom: 25px;
            transition: 0.3s;
        }

        .section-img:hover {
            transform: translateY(-6px);
            box-shadow: 0 15px 40px rgba(0,0,0,0.15);
        }

        /* IMAGE */
        .section-img {
            width: 100%;
            height: auto;          
            border-radius: 14px;
            object-fit: contain;   
            background: #f1f5f9;   
            cursor: zoom-in;
        }

        /* TITLE */
        .section-title {
            font-size: 20px;
            font-weight: 700;
            margin-bottom: 10px;
        }

        /* GROUP */
        .group-title {
            font-weight: 600;
            padding: 10px;
            border-radius: 10px;
            cursor: pointer;
            background: #f8fafc;
            margin-top: 8px;
            transition: 0.2s;
        }

        .group-title:hover {
            background: #e2e8f0;
        }

        /* ITEMS */
        .group-items {
            margin-left: 10px;
            overflow: hidden;
            max-height: 0;
            transition: max-height 0.4s ease;
        }

        /* ITEM */
        .item-row {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 8px 10px;
            margin-top: 6px;
            border-radius: 8px;
            transition: 0.2s;
        }

        .item-row:hover {
            background: #f1f5f9;
        }

        /* DOWNLOAD BUTTON */
        .download-btn {
            font-size: 12px;
            border-radius: 20px;
        }

        /* ARROW */
        .arrow {
            float: right;
            transition: 0.3s;
        }

        .rotate {
            transform: rotate(90deg);
        }

    /*===============header=====================*/
    .sop-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        border-bottom: 1px solid #eee;
        padding-bottom: 10px;
    }

    .sop-title {
        font-size: 26px;
        font-weight: 700;
        background: linear-gradient(90deg, #ea580c, #f97316);
        -webkit-background-clip: text;
        -webkit-text-fill-color: transparent;
    }

    .sop-subtitle {
        font-size: 13px;
        color: #64748b;
        margin-top: 2px;
    }

    .section-banner {
        background: linear-gradient(135deg, #f97316, #ea580c);
        color: white;
        padding: 14px 18px;
        border-radius: 12px;
        margin-bottom: 15px;
        box-shadow: 0 6px 20px rgba(249,115,22,0.3);
    }

    /* TITLE BESAR */
    .section-name {
        font-size: 18px;
        font-weight: 700;
        letter-spacing: 0.5px;
    }

    /* SUBTEXT */
    .section-desc {
        font-size: 12px;
        opacity: 0.9;
    }

    .section-card:hover .section-banner {
        transform: scale(1.01);
        transition: 0.2s;
    }

    /**/
    /* RIGHT SIDE */
    .sop-right {
        padding: 10px 20px;
    }

    /* CENTER TITLE */
    .sop-header-center {
        text-align: center;
        margin-bottom: 20px;
    }

    .main-title {
        font-size: 26px;
        font-weight: 700;
        color: #1e293b;
    }

    .sub-title {
        font-size: 14px;
        color: #0ea5e9;
        margin-top: 5px;
        letter-spacing: 1px;
    }

    .underline {
        width: 120px;
        height: 3px;
        background: #0ea5e9;
        margin: 8px auto 0;
        border-radius: 2px;
    }

    /* GROUP STYLE (BUTTON LOOK) */
    .group-box {
        margin-top: 15px;
    }

    .group-header {
        background: linear-gradient(135deg, #0f172a, #0ea5e9);
        color: white;
        padding: 14px 18px;
        border-radius: 12px;
        font-weight: 600;
        cursor: pointer;
        display: flex;
        justify-content: space-between;
        align-items: center;
        transition: 0.3s;
    }

    .group-header:hover {
        transform: translateY(-2px);
        box-shadow: 0 8px 20px rgba(14,165,233,0.3);
    }

    /* ITEMS */
    .group-items {
        max-height: 0;
        overflow: hidden;
        transition: 0.4s ease;
        margin-top: 5px;
    }

    /* ITEM MODERN */
    .item-row-modern {
        display: flex;
        justify-content: space-between;
        align-items: center;
        background: #f1f5f9;
        padding: 10px 12px;
        border-radius: 10px;
        margin-top: 8px;
    }

    .item-row-modern a {
        font-size: 12px;
        text-decoration: none;
        background: #0ea5e9;
        color: white;
        padding: 4px 10px;
        border-radius: 6px;
    }

    .item-row-modern a:hover {
        background: #0284c7;
    }

    /* ARROW */
    .arrow {
        transition: 0.3s;
    }

    .rotate {
        transform: rotate(180deg);
    }

    /*IMAGE Style*/
    /* MODAL BACKDROP */
    .img-modal {
        display: none;
        position: fixed;
        z-index: 9999;
        left: 0;
        top: 0;
        width: 100%;
        height: 100%;
        background: rgba(30, 41, 59, 0.65);
        backdrop-filter: blur(3px);
        align-items: center;
        justify-content: center;
    }

    .img-modal.show {
        display: flex;
    }

    /* CONTENT BOX (putih, ada padding, rounded, shadow) */
    .img-modal-box {
        position: relative;
        background: #ffffff;
        border-radius: 16px;
        padding: 20px;
        width: 40%;              /* dari 80% + max-width 1000px jadi full 90% layar */
        max-height: 90vh;
        overflow-y: auto;
        box-shadow: 0 20px 60px rgba(0, 0, 0, 0.35);
        animation: zoomIn 0.25s ease;
    }

    /* IMAGE DI DALAM BOX */
    .img-modal-content {
        width: 100%;
        height: auto;             /* auto mengikuti rasio asli gambar */
        display: block;
        border-radius: 10px;
        object-fit: contain;
    }

    /* TOMBOL BACK/CLOSE */
    .img-back-btn {
        margin-top: 14px;
        display: inline-flex;
        align-items: center;
        gap: 6px;
        background: #0f172a;
        color: white;
        border: none;
        padding: 8px 18px;
        border-radius: 30px;
        font-size: 13px;
        font-weight: 600;
        cursor: pointer;
        transition: 0.2s;
    }

    .img-back-btn:hover {
        background: #334155;
        transform: translateY(-2px);
    }

    @keyframes zoomIn {
        from { transform: scale(0.9); opacity: 0; }
        to { transform: scale(1); opacity: 1; }
    }

    /* CLOSE BUTTON */
    .img-close {
        position: absolute;
        top: 20px;
        right: 35px;
        color: white;
        font-size: 35px;
        font-weight: bold;
        cursor: pointer;
    }

    /* ANIMASI */
    @keyframes zoomIn {
        from { transform: scale(0.7); opacity: 0; }
        to { transform: scale(1); opacity: 1; }
    }

    /* =============== Preview ================*/
    .modal-dialog {
        width: 90%;
        max-width: 1200px;
    }
    .modal-content {
        max-height: 700px;
        height: 50%;
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
    .item-row-modern a + a {
        margin-left: 6px;
    }

    /* Kalau gambar landscape (lebih lebar dari tinggi): gambar di atas, isi di bawah */
    .row.g-3.layout-landscape {
        flex-direction: column;
    }

    .row.g-3.layout-landscape > .col-md-4,
    .row.g-3.layout-landscape > .col-md-8 {
        flex: 0 0 100%;
        max-width: 100%;
        width: 100%;
    }

    .row.g-3.layout-landscape > .col-md-4 {
        display: flex;
        justify-content: center;
    }

    .row.g-3.layout-landscape .section-img {
        width: auto;
        max-width: 100%;
        max-height: 420px;   /* biar tidak terlalu tinggi meski lebar penuh */
    }
    </style>

    <div class="sop-wrapper sop-header mb-4">
        <div>
            <asp:Label ID="lblMenuTitle" runat="server" CssClass="sop-title"></asp:Label>
            <div class="sop-subtitle">Standard Operating Procedure</div>
        </div>
    </div>

    <div class="sop-wrapper">
        <div id="sopContainer" runat="server"></div>

        <div id="imgModal" class="img-modal" onclick="closeImageModal()">
            <div class="img-modal-box" onclick="event.stopPropagation()">
                <img class="img-modal-content" id="imgPreview" />
                <button type="button" class="img-back-btn" onclick="closeImageModal()">
                    ← Back
                </button>
            </div>
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
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") {
                closeImageModal();
            }
        });

        function checkImageOrientation(img) {
            // jaga-jaga kalau gambar gagal load / dimensi 0
            if (!img.naturalWidth || !img.naturalHeight) return;

            var row = img.closest('.row.g-3');
            if (!row) return;

            if (img.naturalWidth > img.naturalHeight) {
                row.classList.add('layout-landscape');
            } else {
                row.classList.remove('layout-landscape');
            }
        }

        function toggleGroup(el) {
            let items = el.querySelector(".group-items");
            let arrow = el.querySelector(".arrow");

            if (items.style.maxHeight) {
                items.style.maxHeight = null;
                arrow.classList.remove("rotate");
            } else {
                items.style.maxHeight = items.scrollHeight + "px";
                arrow.classList.add("rotate");
            }
        }

        function openImageModal(src) {
            let modal = document.getElementById("imgModal");
            let img = document.getElementById("imgPreview");

            img.src = src;
            modal.classList.add("show");
        }

        function closeImageModal() {
            document.getElementById("imgModal").classList.remove("show");
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

            if (modalBody && frame) {
                frame.style.height = modalBody.clientHeight + "px";
            }
        }

        function showNoAccess() {
            var modal = new bootstrap.Modal(document.getElementById('noAccessModal'));
            modal.show();
        }

        window.addEventListener("resize", resizePreview);

        document.getElementById('previewModal')
            .addEventListener('shown.bs.modal', resizePreview);

        document.getElementById('previewModal')
            .addEventListener('hidden.bs.modal', function () {
                document.getElementById("previewFrame").src = "";
            });
    </script>

</asp:Content>