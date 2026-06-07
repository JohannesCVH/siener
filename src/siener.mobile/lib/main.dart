import 'package:flutter/material.dart';
import 'package:flutter_webrtc/flutter_webrtc.dart';
import 'package:siener.mobile/siener_http_client.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';

void main() async {
  await dotenv.load(fileName: ".env");
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return const MaterialApp(home: HomePage(),);
  }
}

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  String message = "Hello World!";
  
  void _updateMessage() {
    setState(() {
      message = "You clicked!";
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: WebRTCStreamPlayer()
      ),
    );
  }
}

class WebRTCStreamPlayer extends StatefulWidget {
  final String whepUrl = dotenv.get('WHEP_URL');

  WebRTCStreamPlayer({super.key});

  @override
  State<WebRTCStreamPlayer> createState() => _WebRTCStreamPlayerState();
}

class _WebRTCStreamPlayerState extends State<WebRTCStreamPlayer> {
  final RTCVideoRenderer _renderer = RTCVideoRenderer();
  RTCPeerConnection? _peerConnection;

  @override
  void initState() {
    super.initState();
    _initRenderer();
  }

  Future<void> _initRenderer() async {
    await _renderer.initialize();
    _connect();
  }

  Future<void> _connect() async {
    final httpClient = await getSienerHttpClient();
    _peerConnection = await createPeerConnection({});

    // Hook up the video stream
    _peerConnection!.onTrack = (event) {
      print('onTrack event received! Track kind: ${event.track.kind}');
      if (event.track.kind == 'video') {
        _renderer.srcObject = event.streams[0];
        setState(() {});
      }
    };

    // Transceiver setup
    await _peerConnection!.addTransceiver(
        kind: RTCRtpMediaType.RTCRtpMediaTypeVideo,
        init: RTCRtpTransceiverInit(direction: TransceiverDirection.RecvOnly));

    // Create Offer
    final offer = await _peerConnection!.createOffer();
    await _peerConnection!.setLocalDescription(offer);

    // Send to MediaMTX WHEP endpoint
    final response = await httpClient.post(
      Uri.parse(widget.whepUrl),
      headers: {'Content-Type': 'application/sdp'},
      body: offer.sdp,
    );

    // Handle Answer
    if (response.statusCode == 201) {
      final String answerSdp = response.body;
      await _peerConnection!.setRemoteDescription(
        RTCSessionDescription(answerSdp, 'answer'),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    const double padding = 16.0;
    final double screenWidth = MediaQuery.of(context).size.width;
    final double videoWidth = screenWidth - (padding * 2);

    return Center(
      child: SizedBox(
        width: videoWidth,
        child: AspectRatio(
          aspectRatio: 16/9,
          child: RTCVideoView(
            _renderer,
            objectFit: RTCVideoViewObjectFit.RTCVideoViewObjectFitCover,
          ),
        )
      ),
    );
  }
}