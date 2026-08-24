import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_webrtc/flutter_webrtc.dart';
import 'package:sensors_plus/sensors_plus.dart';
import 'package:siener.client/services/stream_player_service.dart';

class FullScreenStreamPlayerWidget extends StatefulWidget {
  final StreamPlayerService _streamPlayerService;

  const FullScreenStreamPlayerWidget({super.key, required this._streamPlayerService});

  @override
  State<FullScreenStreamPlayerWidget> createState() => _FullScreenStreamPlayerWidgetState();
}

class _FullScreenStreamPlayerWidgetState extends State<FullScreenStreamPlayerWidget> {
  StreamSubscription<AccelerometerEvent>? _accelerometerSubscription;
  
  @override
  void initState() {
    super.initState();
    
    SystemChrome.setPreferredOrientations([
      DeviceOrientation.landscapeLeft,
      DeviceOrientation.landscapeRight,
    ]);

    // Hide Status bar and Bottom Navigation Bar (Immersive Full Screen)
    SystemChrome.setEnabledSystemUIMode(SystemUiMode.immersiveSticky);

    //When portrait exit full screen
    _accelerometerSubscription = accelerometerEventStream().listen((event) {
      if (event.y.abs() > 7.0 && event.x.abs() < 4.0) {
        if (ModalRoute.of(context)?.isCurrent == true) {
          _exitFullScreen(context);
        }
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.black,
      body: Stack(
        children: [
          Center(
            child: AspectRatio(
              aspectRatio: 16 / 9,
              child: RTCVideoView(
                widget._streamPlayerService.renderer,
                objectFit: RTCVideoViewObjectFit.RTCVideoViewObjectFitContain,
              ),
            ),
          ),
        ],
      ),
    );
  }

  void _exitFullScreen(BuildContext context) {
    Navigator.of(context).pop();
  }

  @override
  void dispose() {
    _accelerometerSubscription?.cancel();

    // Restore standard device notification/navigation overlays
    SystemChrome.setEnabledSystemUIMode(SystemUiMode.edgeToEdge);
    super.dispose();
  }
}