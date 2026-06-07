import 'dart:io';

import 'package:flutter/services.dart';
import 'package:http/http.dart' as http;
import 'package:http/io_client.dart';

Future<http.Client> getSienerHttpClient() async {
  final certData = await rootBundle.load('./assets/certs/rootCA.pem');

  final secContext = SecurityContext(withTrustedRoots: false);
  secContext.setTrustedCertificatesBytes(certData.buffer.asUint8List());

  final httpClient = HttpClient(context: secContext);
  return IOClient(httpClient);
}