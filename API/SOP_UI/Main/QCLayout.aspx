<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="QCLayout.aspx.cs" Inherits="SOP_UI.Main.QCLayout" Async="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        body {
            background: radial-gradient(circle at top left, #eef2ff, #e0f2fe 45%, #f8fafc 100%);
            background-attachment: fixed;
        }

        .style-wrapper {
            padding-bottom: 40px;
        }

        .page-header {
            display: flex;
            align-items: center;
            gap: 12px;
            margin-bottom: 8px;
        }

        .page-header h3 {
            margin: 0;
            font-weight: 800;
            background: linear-gradient(90deg, #4f46e5, #0ea5e9);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }

        .page-subtitle {
            color: #64748b;
            font-size: 13px;
            margin-bottom: 20px;
        }

        /* CARD */
        .style-card {
            background: white;
            border-radius: 20px;
            overflow: hidden;
            box-shadow: 0 6px 20px rgba(15,23,42,0.06);
            transition: transform 0.25s ease, box-shadow 0.25s ease;
            opacity: 0;
            transform: translateY(16px);
            animation: cardFadeIn 0.5s ease forwards;
            height: 100%;
        }

        @keyframes cardFadeIn {
            to { opacity: 1; transform: translateY(0); }
        }

        .style-card:hover {
            transform: translateY(-8px);
            box-shadow: 0 20px 40px rgba(15,23,42,0.14);
        }

        /* THUMBNAIL */
        .style-thumb-wrap {
            position: relative;
            height: 190px;
            background: linear-gradient(135deg, #0f172a, #1e293b);
            overflow: hidden;
        }

        .style-thumb {
            width: 100%;
            height: 100%;
            object-fit: cover;
            cursor: pointer;
            transition: transform 0.5s ease, filter 0.3s ease;
        }

        .style-thumb-wrap:hover .style-thumb {
            transform: scale(1.08);
            filter: brightness(0.65);
        }

        .style-thumb-wrap::after {
            content: '';
            position: absolute;
            inset: 0;
            background: linear-gradient(to top, rgba(0,0,0,0.35), transparent 50%);
            pointer-events: none;
        }

        .style-thumb-fallback {
            width: 100%;
            height: 100%;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            font-size: 42px;
            background: linear-gradient(135deg, #f1f5f9, #e2e8f0);
            color: #94a3b8;
            text-align: center;
            gap: 4px;
        }

        .play-overlay {
            position: absolute;
            top: 50%; left: 50%;
            transform: translate(-50%, -50%) scale(1);
            width: 56px; height: 56px;
            border-radius: 50%;
            background: rgba(255,255,255,0.95);
            display: flex; align-items: center; justify-content: center;
            font-size: 20px;
            color: #4f46e5;
            cursor: pointer;
            box-shadow: 0 6px 20px rgba(0,0,0,0.25);
            transition: transform 0.25s ease;
            animation: pulseRing 2.5s infinite;
        }

        .play-overlay:hover {
            transform: translate(-50%, -50%) scale(1.15);
        }

        @keyframes pulseRing {
            0%   { box-shadow: 0 6px 20px rgba(0,0,0,0.25), 0 0 0 0 rgba(255,255,255,0.5); }
            70%  { box-shadow: 0 6px 20px rgba(0,0,0,0.25), 0 0 0 14px rgba(255,255,255,0); }
            100% { box-shadow: 0 6px 20px rgba(0,0,0,0.25), 0 0 0 0 rgba(255,255,255,0); }
        }

        /* BADGE di pojok thumbnail */
        .style-badges {
            position: absolute;
            top: 10px; left: 10px;
            display: flex;
            gap: 6px;
            z-index: 2;
        }

        .style-badge {
            font-size: 10px;
            font-weight: 700;
            padding: 3px 9px;
            border-radius: 20px;
            background: rgba(255,255,255,0.9);
            color: #1e293b;
            letter-spacing: 0.3px;
        }

        /* BODY */
        .style-body {
            padding: 16px 18px 18px;
        }

        .style-title {
            font-size: 16px;
            font-weight: 700;
            color: #1e293b;
            margin-bottom: 4px;
        }

        .style-desc {
            font-size: 13px;
            color: #64748b;
            margin: 0 0 14px;
            display: -webkit-box;
            -webkit-line-clamp: 2;
            -webkit-box-orient: vertical;
            overflow: hidden;
            line-height: 1.5;
        }

        .style-actions {
            display: flex;
            gap: 8px;
            flex-wrap: wrap;
        }

        .btn-action {
            font-size: 12px;
            text-decoration: none;
            padding: 7px 14px;
            border-radius: 20px;
            font-weight: 600;
            display: inline-flex;
            align-items: center;
            gap: 5px;
            transition: 0.2s;
        }

        .btn-video {
            background: #eef2ff;
            color: #4f46e5;
        }
        .btn-video:hover {
            background: #4f46e5;
            color: white;
            transform: translateY(-2px);
        }

        .btn-file {
            background: #e0f2fe;
            color: #0284c7;
        }
        .btn-file:hover {
            background: #0ea5e9;
            color: white;
            transform: translateY(-2px);
        }
        #videoModal video { width: 100%; max-height: 80vh; }
        .modal-video{
            --bs-modal-width: 80% !important;
        }

        .qc-filters {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
            margin-bottom: 22px;
        }

        .qc-filter-select {
            padding: 9px 14px;
            border-radius: 30px;
            border: 1px solid #cbd5e1;
            outline: none;
            font-size: 13px;
            background: white;
            box-shadow: 0 2px 10px rgba(0,0,0,0.04);
            transition: 0.2s;
        }

        .qc-filter-select:focus {
            border-color: #6366f1;
            box-shadow: 0 0 0 4px rgba(99,102,241,0.15);
        }

        .style-meta {
            font-size: 12px;
            color: #686868;
            font-weight: 600;
            margin-bottom: 6px;
            background-color : #bfddff;
            border-radius : 25px;
            padding : 5px 10px;
        }
    </style>

    <div class="style-wrapper">
        <div class="container mt-4">
            <div class="page-header">
                <span style="font-size:28px;">🎬</span>
                <h3>QC Inspection Layout</h3>
            </div>
            <div class="page-subtitle">Kumpulan video & dokumen inspeksi QC per style</div>

            <div class="qc-filters" id="filterBar" runat="server"></div>

            <div class="row" id="styleContainer" runat="server"></div>
        </div>
    </div>

    <asp:HiddenField ID="hfHasFileAccess" runat="server" />

    <div class="modal fade" id="videoModal" tabindex="-1">
      <div class="modal-dialog modal-dialog-centered modal-video">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Video</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
          </div>
          <div class="modal-body p-0">
            <video id="videoPlayer" controls controlsList="nodownload noplaybackrate" disablePictureInPicture></video>
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
            Mengerti
          </button>
        </div>
      </div>
    </div>

    <script>
        // Blokir klik kanan (context menu) di kartu video & popup video.
        // Pakai event delegation di document supaya tetap berlaku untuk
        // kartu-kartu yang dibuat dinamis oleh server-side (styleContainer.InnerHtml).
        document.addEventListener('contextmenu', function (e) {
            if (e.target.closest('.style-card') || e.target.closest('#videoModal')) {
                e.preventDefault();
            }
        });

        function openVideo(url) {
            var player = document.getElementById("videoPlayer");
            player.src = url;

            var modal = new bootstrap.Modal(document.getElementById('videoModal'));
            modal.show();

            player.play().catch(() => { });
        }

        function showNoAccess() {
            var modal = new bootstrap.Modal(document.getElementById('noAccessModal'));
            modal.show();
        }

        document.getElementById('videoModal').addEventListener('hidden.bs.modal', function () {
            var player = document.getElementById("videoPlayer");
            player.pause();
            player.src = "";
        });

        function searchStyle() {
            function val(id) {
                var el = document.getElementById(id);
                return el ? el.value.toLowerCase() : "";
            }

            let fBuyer = val("filterBuyer");
            let fStyle = val("filterStyle");
            let fItem = val("filterItem");
            let fSort = val("filterSort");

            let anyVisible = false;

            document.querySelectorAll(".style-card").forEach(card => {
                let wrapper = card.closest(".col-lg-4, .col-md-6");

                let match = (!fBuyer || (card.dataset.buyer || "").toLowerCase().includes(fBuyer))
                    && (!fStyle || (card.dataset.style || "").toLowerCase().includes(fStyle))
                    && (!fItem || (card.dataset.item || "").toLowerCase().includes(fItem))
                    && (!fSort || (card.dataset.sort || "").toLowerCase().includes(fSort));

                wrapper.style.display = match ? "" : "none";
                if (match) anyVisible = true;
            });

            let emptyMsg = document.getElementById("qcEmptySearch");
            if (!anyVisible) {
                if (!emptyMsg) {
                    emptyMsg = document.createElement("div");
                    emptyMsg.id = "qcEmptySearch";
                    emptyMsg.className = "col-12 text-center py-5";
                    emptyMsg.style.color = "#64748b";
                    emptyMsg.innerHTML = "🔍 Style tidak ditemukan";
                    document.getElementById("styleContainer").appendChild(emptyMsg);
                }
            } else if (emptyMsg) {
                emptyMsg.remove();
            }
        }
    </script>
</asp:Content>
