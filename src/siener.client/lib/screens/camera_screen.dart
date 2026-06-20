import 'package:flutter/material.dart';
import 'package:siener.client/widgets/stream_player_widget.dart';

class CameraScreen extends StatelessWidget {
  const CameraScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Camera'),
      ),
      body: Column(
        children: [
          Center(child: StreamPlayerWidget())
        ],
      ),
    );
  }
}