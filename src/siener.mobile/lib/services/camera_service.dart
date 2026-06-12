import 'dart:convert';

import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:siener.mobile/models/camera.dart';
import 'package:siener.mobile/http_client.dart';

class CameraService {
  final String _baseUrl;

  CameraService(): _baseUrl = dotenv.get('API_BASE_URL');

  Future<List<Camera>> fetchCameras() async {
    final client = await getHttpClientWithCert();
    final response = await client.get(Uri.parse('$_baseUrl/api/Camera/Streams'));

    if (response.statusCode == 200) {
      final List<dynamic> body = jsonDecode(response.body);
      return body.map((dynamic item) => Camera.fromJson(item)).toList();
    } else {
      throw Exception('Failed to load cameras');
    }
  }

  String getThumbnailUrl(String cameraName) {
    return '$_baseUrl/api/Camera/GetThumbnail/$cameraName';
  }
}