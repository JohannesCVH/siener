import 'dart:async';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:flutter_webrtc/flutter_webrtc.dart';
import 'package:siener.client/http_client.dart';
import 'package:siener.client/models/camera.dart';

class StreamPlayerService {
  final Camera camera;
  final finalApiBaseUrl = dotenv.get('API_BASE_URL');

  final RTCVideoRenderer renderer = RTCVideoRenderer();
  RTCPeerConnection? _peerConnection;

  bool isStreamReady = false;
  bool isRotateable = true;

  StreamPlayerService({required this.camera});

  Future<void> initialize() async {
    await renderer.initialize();
    await _connect();
  }

  Future<void> _connect() async {
    final httpClient = await getHttpClientWithCert();
    _peerConnection = await createPeerConnection({});

    _peerConnection!.onTrack = (event) {
      if (event.track.kind == 'video') {
        renderer.srcObject = event.streams[0];
      }
    };

    await _peerConnection!.addTransceiver(
      kind: RTCRtpMediaType.RTCRtpMediaTypeVideo,
      init: RTCRtpTransceiverInit(direction: TransceiverDirection.RecvOnly),
    );

    final offer = await _peerConnection!.createOffer();
    await _peerConnection!.setLocalDescription(offer);

    try {
      final response = await httpClient.post(
        Uri.parse('$finalApiBaseUrl:${dotenv.get('WEB_RTC_PORT')}/${camera.name}/whep'),
        headers: {'Content-Type': 'application/sdp'},
        body: offer.sdp,
      );

      if (response.statusCode == 201) {
        final String answerSdp = response.body;
        await _peerConnection!.setRemoteDescription(
          RTCSessionDescription(answerSdp, 'answer'),
        );
      }
    } catch (exception) {
      print('MediaMTX WHEP POST Error: $exception');
    }
  }

  void dispose() {
    _peerConnection?.close();
    renderer.dispose();
  }
}