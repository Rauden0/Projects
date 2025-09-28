import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/material.dart';
import 'package:tes/models/base_model.dart';

class BaseService<T extends BaseModel> {
  final String collectionPath;
  final T Function(Map<String, dynamic> json) fromJson;
  @protected
  late CollectionReference<T> collection;

  BaseService({required this.collectionPath, required this.fromJson}) {
    _initializeCollection();
    FirebaseAuth.instance.authStateChanges().listen((user) {
      if (user != null) {
        _initializeCollection();
      }
    });
  }

  void _initializeCollection() {
    final userId = FirebaseAuth.instance.currentUser?.uid;
    if (userId != null) {
      final path = 'users/$userId/$collectionPath';
      collection = FirebaseFirestore.instance.collection(path).withConverter(
        fromFirestore: (snapshot, options) {
          final json = snapshot.data() ?? {};
          json['id'] = snapshot.id;
          return fromJson(json);
        },
        toFirestore: (entity, options) {
          final json = entity.toJson();
          json.remove('id');
          return json;
        },
      );
    }
  }

  Stream<List<T>> get stream {
    return collection.snapshots().map((snapshot) {
      return snapshot.docs.map((doc) => doc.data()).toList();
    });
  }

  Stream<T?> streamById(String id) {
    return collection.doc(id).snapshots().map((snapshot) => snapshot.data());
  }

  Future<void> drop() async {
    await collection.get().then((snapshot) {
      for (var doc in snapshot.docs) {
        doc.reference.delete();
      }
    });
  }

  Future<DocumentReference<T>> add(T entity) {
    return collection.add(entity);
  }

  Future<void> delete(T entity) {
    return collection.doc(entity.id).delete();
  }

  Future<void> update(T entity) {
    return collection.doc(entity.id).set(entity);
  }
}
