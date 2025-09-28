/// GENERATED CODE - DO NOT MODIFY BY HAND
/// *****************************************************
///  FlutterGen
/// *****************************************************

// coverage:ignore-file
// ignore_for_file: type=lint
// ignore_for_file: directives_ordering,unnecessary_import,implicit_dynamic_list_literal,deprecated_member_use

import 'package:flutter/widgets.dart';

class $AssetsImagesGen {
  const $AssetsImagesGen();

  /// Directory path: assets/images/player_avatars
  $AssetsImagesPlayerAvatarsGen get playerAvatars =>
      const $AssetsImagesPlayerAvatarsGen();
}

class $AssetsImagesPlayerAvatarsGen {
  const $AssetsImagesPlayerAvatarsGen();

  /// File path: assets/images/player_avatars/barbarian.jpg
  AssetGenImage get barbarian =>
      const AssetGenImage('assets/images/player_avatars/barbarian.jpg');

  /// File path: assets/images/player_avatars/bard.jpg
  AssetGenImage get bard =>
      const AssetGenImage('assets/images/player_avatars/bard.jpg');

  /// File path: assets/images/player_avatars/cleric.jpg
  AssetGenImage get cleric =>
      const AssetGenImage('assets/images/player_avatars/cleric.jpg');

  /// File path: assets/images/player_avatars/druid.jpg
  AssetGenImage get druid =>
      const AssetGenImage('assets/images/player_avatars/druid.jpg');

  /// File path: assets/images/player_avatars/fighter.jpg
  AssetGenImage get fighter =>
      const AssetGenImage('assets/images/player_avatars/fighter.jpg');

  /// File path: assets/images/player_avatars/monk.jpg
  AssetGenImage get monk =>
      const AssetGenImage('assets/images/player_avatars/monk.jpg');

  /// File path: assets/images/player_avatars/paladin.jpg
  AssetGenImage get paladin =>
      const AssetGenImage('assets/images/player_avatars/paladin.jpg');

  /// File path: assets/images/player_avatars/placeholder.jpg
  AssetGenImage get placeholder =>
      const AssetGenImage('assets/images/player_avatars/placeholder.jpg');

  /// File path: assets/images/player_avatars/ranger.jpg
  AssetGenImage get ranger =>
      const AssetGenImage('assets/images/player_avatars/ranger.jpg');

  /// File path: assets/images/player_avatars/rogue.jpg
  AssetGenImage get rogue =>
      const AssetGenImage('assets/images/player_avatars/rogue.jpg');

  /// File path: assets/images/player_avatars/sorcerer.jpg
  AssetGenImage get sorcerer =>
      const AssetGenImage('assets/images/player_avatars/sorcerer.jpg');

  /// File path: assets/images/player_avatars/warlock.jpg
  AssetGenImage get warlock =>
      const AssetGenImage('assets/images/player_avatars/warlock.jpg');

  /// File path: assets/images/player_avatars/wizard.jpg
  AssetGenImage get wizard =>
      const AssetGenImage('assets/images/player_avatars/wizard.jpg');

  /// List of all assets
  List<AssetGenImage> get values => [
        barbarian,
        bard,
        cleric,
        druid,
        fighter,
        monk,
        paladin,
        placeholder,
        ranger,
        rogue,
        sorcerer,
        warlock,
        wizard
      ];
}

class Assets {
  Assets._();

  static const $AssetsImagesGen images = $AssetsImagesGen();
}

class AssetGenImage {
  const AssetGenImage(
    this._assetName, {
    this.size,
    this.flavors = const {},
  });

  final String _assetName;

  final Size? size;
  final Set<String> flavors;

  Image image({
    Key? key,
    AssetBundle? bundle,
    ImageFrameBuilder? frameBuilder,
    ImageErrorWidgetBuilder? errorBuilder,
    String? semanticLabel,
    bool excludeFromSemantics = false,
    double? scale,
    double? width,
    double? height,
    Color? color,
    Animation<double>? opacity,
    BlendMode? colorBlendMode,
    BoxFit? fit,
    AlignmentGeometry alignment = Alignment.center,
    ImageRepeat repeat = ImageRepeat.noRepeat,
    Rect? centerSlice,
    bool matchTextDirection = false,
    bool gaplessPlayback = true,
    bool isAntiAlias = false,
    String? package,
    FilterQuality filterQuality = FilterQuality.low,
    int? cacheWidth,
    int? cacheHeight,
  }) {
    return Image.asset(
      _assetName,
      key: key,
      bundle: bundle,
      frameBuilder: frameBuilder,
      errorBuilder: errorBuilder,
      semanticLabel: semanticLabel,
      excludeFromSemantics: excludeFromSemantics,
      scale: scale,
      width: width,
      height: height,
      color: color,
      opacity: opacity,
      colorBlendMode: colorBlendMode,
      fit: fit,
      alignment: alignment,
      repeat: repeat,
      centerSlice: centerSlice,
      matchTextDirection: matchTextDirection,
      gaplessPlayback: gaplessPlayback,
      isAntiAlias: isAntiAlias,
      package: package,
      filterQuality: filterQuality,
      cacheWidth: cacheWidth,
      cacheHeight: cacheHeight,
    );
  }

  ImageProvider provider({
    AssetBundle? bundle,
    String? package,
  }) {
    return AssetImage(
      _assetName,
      bundle: bundle,
      package: package,
    );
  }

  String get path => _assetName;

  String get keyName => _assetName;
}
