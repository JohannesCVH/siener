import 'dart:convert';
import 'dart:typed_data';

import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:siener.client/logger.dart';
import 'package:siener.client/models/camera.dart';
import 'package:siener.client/http_client.dart';

class CameraService {
  final String _baseUrl;
  final int _basePort;

  CameraService(): 
    _baseUrl = dotenv.get('API_BASE_URL'), 
    _basePort = int.parse(dotenv.get('API_BASE_PORT'));

  Future<List<Camera>?> fetchCameras() async {
    final client = await getHttpClientWithCert();

    try {
      final response = await client.get(Uri.parse('$_baseUrl:$_basePort/api/Camera/Streams'));
      
      if (response.statusCode == 200) {
        final List<dynamic> body = jsonDecode(response.body);
        return body.map((dynamic item) => Camera.fromJson(item)).toList();
      }
    } catch (e) {
      logError(CameraService, 'fetchCameras', 'Failed to load cameras: $e');
    }

    return null;
  }

  String getThumbnailUrl(String cameraName) {
    String url = '$_baseUrl:$_basePort/api/Camera/GetThumbnail/$cameraName';
    logMessage(CameraService, 'getThumbnailUrl', 'Generated thumbnail URL: $url');
    return url;
  }

  Future<Uint8List?> getThumbnailBytes(String cameraName) async {
    final client = await getHttpClientWithCert();
    try {
      final response = await client.get(Uri.parse(getThumbnailUrl(cameraName.split('_').first)));
      if (response.statusCode == 200) {
        return response.bodyBytes;
      }
    } catch (e) {
      logError(CameraService, 'getThumbnailBytes', 'Failed: $e');
    }
    return null;
  }
}