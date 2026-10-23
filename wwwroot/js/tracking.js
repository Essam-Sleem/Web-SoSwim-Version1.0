/**
 * SOSWIM Swimmer Live Tracking & Telemetry Dashboard Script
 * Handles real-time vital telemetry polling, SOS rescue triggering,
 * and live prototype state simulation. Supports AR & EN.
 */

document.addEventListener('DOMContentLoaded', function () {
    const activeTrackingCodeInput = document.getElementById('activeTrackingCode');
    if (!activeTrackingCodeInput) return;

    const isArabic = document.documentElement.lang !== 'en';
    const currentCode = activeTrackingCodeInput.value;
    const btnParentSos = document.getElementById('btnParentSos');
    const riskPercentageFill = document.getElementById('riskPercentageFill');
    const riskPercentageText = document.getElementById('riskPercentageText');
    const riskStatusHint = document.getElementById('riskStatusHint');
    const swimmerStatusBadge = document.getElementById('swimmerStatusBadge');
    const swimmerStatusLabel = document.getElementById('swimmerStatusLabel');
    const metricHeartRate = document.getElementById('metricHeartRate');
    const metricMovement = document.getElementById('metricMovement');
    const metricDepth = document.getElementById('metricDepth');
    const metricDistance = document.getElementById('metricDistance');
    const metricBattery = document.getElementById('metricBattery');
    const swimmerBeacon = document.getElementById('swimmerBeacon');
    const sosCardWrapper = document.getElementById('sosCardWrapper');
    const sosAlertBanner = document.getElementById('sosAlertBanner');
    const sosAlertBannerText = document.getElementById('sosAlertBannerText');

    // Handle Parent SOS Click
    if (btnParentSos) {
        btnParentSos.addEventListener('click', async function () {
            if (btnParentSos.disabled) return;

            const confirmMsg = isArabic 
                ? 'هل أنت متأكد من رغبتك في إرسال نداء استغاثة عاجل لطاقم الإنقاذ؟ سيتم توجيه قارب الإنقاذ والمنقذين فوراً لموقع السباح.'
                : 'Are you sure you want to transmit an emergency rescue distress alert? Lifeguards and patrol boats will be dispatched immediately.';

            if (!confirm(confirmMsg)) return;

            btnParentSos.disabled = true;
            btnParentSos.innerHTML = `
                <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                ${isArabic ? 'جارٍ إرسال الاستغاثة للبرج...' : 'Transmitting distress signal...'}
            `;

            try {
                const response = await fetch('/Tracking/TriggerParentSos', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        trackingCode: currentCode,
                        reason: isArabic ? 'نداء استغاثة مؤكد من ولي الأمر عبر لوحة المراقبة' : 'Verified parent distress alert from web dashboard'
                    })
                });

                const data = await response.json();
                if (data.success) {
                    showSosSuccess(data.message);
                    if (data.swimmer) {
                        updateSwimmerUI(data.swimmer);
                    }
                } else {
                    alert(data.message || (isArabic ? 'تعذر إرسال الاستغاثة.' : 'Failed to transmit alert.'));
                    if (data.swimmer) {
                        updateSwimmerUI(data.swimmer);
                    }
                }
            } catch (err) {
                console.error('SOS Trigger Error:', err);
                alert(isArabic ? 'حدث خطأ في الاتصال بالخادم. يرجى التوجه لبرج الإنقاذ مباشرة.' : 'Server connection error. Please contact lifeguard tower directly.');
            }
        });
    }

    // Prototype Scenario Simulation Buttons
    const scenarioButtons = document.querySelectorAll('.btn-simulate-scenario');
    scenarioButtons.forEach(btn => {
        btn.addEventListener('click', async function () {
            const scenario = this.getAttribute('data-scenario');
            if (!scenario) return;

            try {
                const response = await fetch('/Tracking/SimulateScenario', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        trackingCode: currentCode,
                        scenario: scenario
                    })
                });

                const data = await response.json();
                if (data.success && data.swimmer) {
                    updateSwimmerUI(data.swimmer);
                }
            } catch (err) {
                console.error('Simulation Switch Error:', err);
            }
        });
    });

    // Update Telemetry & UI
    function updateSwimmerUI(swimmer) {
        if (!swimmer || !swimmer.vitals) return;

        const vitals = swimmer.vitals;
        const risk = vitals.instabilityRiskPercentage;
        const canTriggerSos = risk >= 50;

        // Vitals metrics
        if (metricHeartRate) metricHeartRate.textContent = vitals.heartRateBpm;
        if (metricMovement) metricMovement.textContent = vitals.movementPattern;
        if (metricDepth) metricDepth.textContent = vitals.depthMeters.toFixed(1);
        if (metricDistance) metricDistance.textContent = vitals.distanceFromShoreMeters.toFixed(1);
        if (metricBattery) metricBattery.textContent = vitals.batteryPercentage + '%';

        // Risk bar
        if (riskPercentageFill) {
            riskPercentageFill.style.width = risk + '%';
            if (risk < 50) {
                riskPercentageFill.style.background = 'linear-gradient(90deg, #10b981, #0284c7)';
            } else if (risk < 80) {
                riskPercentageFill.style.background = 'linear-gradient(90deg, #0284c7, #f59e0b)';
            } else {
                riskPercentageFill.style.background = 'linear-gradient(90deg, #f59e0b, #ef4444)';
            }
        }

        if (riskPercentageText) {
            riskPercentageText.textContent = risk + '%';
            if (risk < 50) {
                riskPercentageText.className = 'metric-value text-emerald';
            } else if (risk < 80) {
                riskPercentageText.className = 'metric-value text-amber';
            } else {
                riskPercentageText.className = 'metric-value text-danger';
            }
        }

        // Parent SOS button lock/unlock state (The 50% Rule)
        if (btnParentSos) {
            if (swimmer.isParentAlertActive || swimmer.status === 3) {
                btnParentSos.disabled = true;
                btnParentSos.innerHTML = `
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>
                    <span>${isArabic ? 'تم إرسال الاستغاثة بنجاح - فرق الإنقاذ في الطريق' : 'SOS Alert Sent - Lifeguards Dispatched'}</span>
                `;
                btnParentSos.className = 'btn btn-success fw-bold p-3 w-100';
                if (sosCardWrapper) sosCardWrapper.className = 'sos-button-card unlocked';
                if (riskStatusHint) {
                    riskStatusHint.innerHTML = `<span class="text-danger fw-bold">${isArabic ? 'الإنقاذ في حالة تحرك قصوى للموقع الآن.' : 'Lifeguards are en route to swimmer location.'}</span>`;
                }
            } else if (canTriggerSos) {
                btnParentSos.disabled = false;
                btnParentSos.className = 'btn-parent-sos w-100 justify-content-center';
                btnParentSos.innerHTML = `
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line></svg>
                    <span>${isArabic ? 'إرسال نداء استغاثة عاجل للمنقذ' : 'Transmit Emergency Distress Call'}</span>
                `;
                if (sosCardWrapper) sosCardWrapper.className = 'sos-button-card unlocked';
                if (riskStatusHint) {
                    riskStatusHint.innerHTML = `<span class="text-amber fw-bold">${isArabic ? 'مؤشرات عدم الاستقرار تجاوزت 50%! تم فتح زر الاستغاثة إذا كنت قلقاً على ابنك.' : 'Instability exceeded 50%! Emergency SOS button is unlocked.'}</span>`;
                }
            } else {
                btnParentSos.disabled = true;
                btnParentSos.className = 'btn-parent-sos w-100 justify-content-center';
                btnParentSos.innerHTML = `
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
                    <span>${isArabic ? 'طلب نجدة طارئة (مقفل حالياً)' : 'Emergency SOS (Currently Locked)'}</span>
                `;
                if (sosCardWrapper) sosCardWrapper.className = 'sos-button-card';
                if (riskStatusHint) {
                    riskStatusHint.innerHTML = `<span class="text-secondary">${isArabic ? 'الزر مقفل تلقائياً لأن مؤشرات السباح مستقرة (أقل من 50% نسبة اشتباه خطر) منعاً للإنذارات الخاطئة.' : 'SOS button locked: Swimmer vitals are stable (< 50% risk) to prevent false alarms.'}</span>`;
                }
            }
        }

        // Status Badge
        if (swimmerStatusBadge && swimmerStatusLabel) {
            if (swimmer.status === 1) { // Stable
                swimmerStatusBadge.className = 'status-badge status-badge-stable';
                swimmerStatusLabel.textContent = isArabic ? 'مستقر وآمن' : 'Stable & Safe';
            } else if (swimmer.status === 2) { // Unstable
                swimmerStatusBadge.className = 'status-badge status-badge-warning';
                swimmerStatusLabel.textContent = isArabic ? 'غير مستقر / اشتباه خطر' : 'Unstable / Needs Attention';
            } else { // InDanger
                swimmerStatusBadge.className = 'status-badge status-badge-danger';
                swimmerStatusLabel.textContent = isArabic ? 'في خطر شديد / استغاثة طارئة' : 'Critical Danger / Active SOS';
            }
        }

        // Sonar Beacon Position Simulation
        if (swimmerBeacon) {
            const maxDistance = 110;
            const dist = Math.min(vitals.distanceFromShoreMeters, maxDistance);
            const bottomPercent = 20 + ((dist / maxDistance) * 58);
            swimmerBeacon.style.bottom = bottomPercent + '%';

            const beaconDot = swimmerBeacon.querySelector('.beacon-core');
            const beaconPulse = swimmerBeacon.querySelector('.beacon-pulse');
            if (beaconDot && beaconPulse) {
                if (swimmer.status === 1) {
                    beaconDot.style.backgroundColor = 'var(--status-stable)';
                    beaconPulse.style.backgroundColor = 'rgba(16, 185, 129, 0.4)';
                } else if (swimmer.status === 2) {
                    beaconDot.style.backgroundColor = 'var(--status-warning)';
                    beaconPulse.style.backgroundColor = 'rgba(245, 158, 11, 0.5)';
                } else {
                    beaconDot.style.backgroundColor = 'var(--status-danger)';
                    beaconPulse.style.backgroundColor = 'rgba(239, 68, 68, 0.6)';
                }
            }
        }
    }

    function showSosSuccess(msg) {
        if (sosAlertBanner && sosAlertBannerText) {
            sosAlertBannerText.textContent = msg;
            sosAlertBanner.classList.remove('d-none');
            sosAlertBanner.scrollIntoView({ behavior: 'smooth' });
        }
    }

    // Auto-poll vitals every 5 seconds to keep telemetry alive
    setInterval(async function () {
        try {
            const res = await fetch(`/Tracking/GetVitals?code=${encodeURIComponent(currentCode)}`);
            if (res.ok) {
                const data = await res.json();
                if (data.success && data.swimmer) {
                    updateSwimmerUI(data.swimmer);
                }
            }
        } catch (e) {
            // Silently continue in background
        }
    }, 5000);
});
